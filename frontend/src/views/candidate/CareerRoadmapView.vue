<script setup>
import { computed, onMounted, ref } from "vue";
import api from "../../services/api";

const goals = ref([]);
const roadmap = ref(null);

const loading = ref(true);
const selecting = ref(false);
const error = ref("");
const success = ref("");

const hasGoals = computed(() => goals.value.length > 0);
const hasRoadmap = computed(() => roadmap.value?.hasCareerGoal);
const hasMissingSkills = computed(() => roadmap.value?.missingSkills?.length > 0);
const hasRecommendedCourses = computed(() => roadmap.value?.recommendedCourses?.length > 0);
const hasPhases = computed(() => roadmap.value?.phases?.length > 0);

const progressPercentage = computed(() => {
    const value = Number(roadmap.value?.progressPercentage) || 0;
    return Math.min(Math.max(value, 0), 100);
});

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const loadRoadmap = async () => {
    const { data } = await api.get("/career-roadmap/my");
    roadmap.value = data;
};

const loadData = async () => {
    loading.value = true;
    clearMessages();

    try {
        const [goalsResponse] = await Promise.all([
            api.get("/career-goals"),
            loadRoadmap(),
        ]);

        goals.value = goalsResponse.data;
    } catch {
        error.value = "Career Roadmap konnte nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const selectGoal = async (goalId) => {
    selecting.value = true;
    clearMessages();

    try {
        await api.post(`/career-goals/select/${goalId}`);
        await loadRoadmap();

        success.value = "Karriereziel erfolgreich ausgewählt.";
    } catch {
        error.value = "Karriereziel konnte nicht gespeichert werden.";
    } finally {
        selecting.value = false;
    }
};

onMounted(loadData);
</script>

<template>
    <div class="container py-4">
        <h1 class="mb-4">🎯 Meine Career Roadmap</h1>

        <div v-if="loading" class="alert alert-info">
            Roadmap wird geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <div v-if="success" class="alert alert-success">
                {{ success }}
            </div>

            <div class="card shadow-sm mb-4">
                <div class="card-body">
                    <h4>Karriereziel auswählen</h4>

                    <div v-if="!hasGoals" class="text-muted">
                        Keine Karriereziele verfügbar.
                    </div>

                    <div v-else class="row">
                        <div v-for="goal in goals" :key="goal.id" class="col-md-6 mb-3">
                            <div class="border rounded p-3 h-100">
                                <h5>{{ goal.name || "Unbekanntes Ziel" }}</h5>

                                <p>
                                    {{ goal.description || "Keine Beschreibung vorhanden." }}
                                </p>

                                <p class="text-muted">
                                    Dauer: {{ goal.durationMonths || 0 }} Monate
                                </p>

                                <button type="button" class="btn btn-primary btn-sm" :disabled="selecting"
                                    @click="selectGoal(goal.id)">
                                    {{ selecting ? "Wird gespeichert..." : "Auswählen" }}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div v-if="hasRoadmap" class="card shadow-sm">
                <div class="card-body">
                    <h3>
                        {{ roadmap.goal?.name || "Unbekanntes Karriereziel" }}
                    </h3>

                    <p>
                        {{ roadmap.goal?.description || "Keine Beschreibung vorhanden." }}
                    </p>

                    <div class="mb-3">
                        <strong>Fortschritt:</strong>
                        {{ progressPercentage }}%
                    </div>

                    <div class="progress mb-4">
                        <div class="progress-bar" role="progressbar" :style="{ width: `${progressPercentage}%` }">
                            {{ progressPercentage }}%
                        </div>
                    </div>

                    <h4>Fehlende Skills</h4>

                    <ul v-if="hasMissingSkills">
                        <li v-for="skill in roadmap.missingSkills" :key="skill.id">
                            {{ skill.name }}
                        </li>
                    </ul>

                    <p v-else class="text-success">
                        Keine fehlenden Skills.
                    </p>

                    <h4>Empfohlene Kurse</h4>

                    <ul v-if="hasRecommendedCourses">
                        <li v-for="course in roadmap.recommendedCourses" :key="course.id">
                            {{ course.title }}
                        </li>
                    </ul>

                    <p v-else class="text-muted">
                        Keine Kursempfehlungen vorhanden.
                    </p>

                    <h4>Lernphasen</h4>

                    <div v-if="hasPhases">
                        <div v-for="phase in roadmap.phases" :key="phase.month" class="card mb-3">
                            <div class="card-body">
                                <h5>
                                    Monat {{ phase.month }}
                                </h5>

                                <ul>
                                    <li v-for="skill in phase.skills" :key="skill.id">
                                        <span v-if="skill.isCompleted" class="text-success">
                                            ✓ {{ skill.name }}
                                        </span>

                                        <span v-else class="text-muted">
                                            ○ {{ skill.name }}
                                        </span>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>

                    <p v-else class="text-muted">
                        Noch keine Lernphasen vorhanden.
                    </p>
                </div>
            </div>

            <div v-else class="alert alert-light border">
                Bitte wähle ein Karriereziel aus, um deine Roadmap zu sehen.
            </div>
        </template>
    </div>
</template>