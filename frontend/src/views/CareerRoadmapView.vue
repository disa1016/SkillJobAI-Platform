<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const goals = ref([]);
const roadmap = ref(null);

const loading = ref(true);
const error = ref("");
const success = ref("");

const loadData = async () => {
    try {
        const goalsResponse = await api.get("/career-goals");
        goals.value = goalsResponse.data;

        const roadmapResponse = await api.get("/career-roadmap/my");
        roadmap.value = roadmapResponse.data;
    } catch {
        error.value = "Career Roadmap konnte nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const selectGoal = async (goalId) => {
    try {
        await api.post(`/career-goals/select/${goalId}`);

        success.value = "Karriereziel erfolgreich ausgewählt.";

        const roadmapResponse = await api.get("/career-roadmap/my");
        roadmap.value = roadmapResponse.data;
    } catch {
        error.value = "Karriereziel konnte nicht gespeichert werden.";
    }
};

onMounted(loadData);
</script>

<template>
    <div class="container py-4">

        <h1 class="mb-4">
            🎯 Meine Career Roadmap
        </h1>

        <div v-if="loading" class="alert alert-info">
            Roadmap wird geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div class="card shadow-sm mb-4">
            <div class="card-body">

                <h4>Karriereziel auswählen</h4>

                <div class="row">
                    <div
                        v-for="goal in goals"
                        :key="goal.id"
                        class="col-md-6 mb-3"
                    >
                        <div class="border rounded p-3 h-100">

                            <h5>{{ goal.name }}</h5>

                            <p>
                                {{ goal.description }}
                            </p>

                            <p class="text-muted">
                                Dauer: {{ goal.durationMonths }} Monate
                            </p>

                            <button
                                class="btn btn-primary btn-sm"
                                @click="selectGoal(goal.id)"
                            >
                                Auswählen
                            </button>

                        </div>
                    </div>
                </div>

            </div>
        </div>

        <div
            v-if="roadmap && roadmap.hasCareerGoal"
            class="card shadow-sm"
        >
            <div class="card-body">

                <h3>
                    {{ roadmap.goal.name }}
                </h3>

                <p>
                    {{ roadmap.goal.description }}
                </p>

                <div class="mb-3">
                    <strong>Fortschritt:</strong>
                    {{ roadmap.progressPercentage }}%
                </div>

                <div class="progress mb-4">
                    <div
                        class="progress-bar"
                        role="progressbar"
                        :style="{ width: roadmap.progressPercentage + '%' }"
                    >
                        {{ roadmap.progressPercentage }}%
                    </div>
                </div>

                <h4>Fehlende Skills</h4>

                <ul>
                    <li
                        v-for="skill in roadmap.missingSkills"
                        :key="skill.id"
                    >
                        {{ skill.name }}
                    </li>
                </ul>

                <h4>Empfohlene Kurse</h4>

                <ul>
                    <li
                        v-for="course in roadmap.recommendedCourses"
                        :key="course.id"
                    >
                        {{ course.title }}
                    </li>
                </ul>

                <h4>Lernphasen</h4>

                <div
                    v-for="phase in roadmap.phases"
                    :key="phase.month"
                    class="card mb-3"
                >
                    <div class="card-body">

                        <h5>
                            Monat {{ phase.month }}
                        </h5>

                        <ul>
                            <li
                                v-for="skill in phase.skills"
                                :key="skill.id"
                            >
                                <span
                                    v-if="skill.isCompleted"
                                    class="text-success"
                                >
                                    ✓ {{ skill.name }}
                                </span>

                                <span
                                    v-else
                                    class="text-muted"
                                >
                                    ○ {{ skill.name }}
                                </span>
                            </li>
                        </ul>

                    </div>
                </div>

            </div>
        </div>

    </div>
</template>