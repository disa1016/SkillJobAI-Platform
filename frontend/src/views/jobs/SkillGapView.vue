<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { getSkillGap } from "@/services/jobService";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";

const route = useRoute();

const skillGap = ref(null);
const loading = ref(true);
const error = ref("");

const hasJobSkills = computed(() => skillGap.value?.jobSkills?.length > 0);
const hasMatchedSkills = computed(() => skillGap.value?.matchedSkills?.length > 0);
const hasMissingSkills = computed(() => skillGap.value?.missingSkills?.length > 0);
const hasRecommendedCourses = computed(() => skillGap.value?.recommendedCourses?.length > 0);

const matchPercentage = computed(() => {
    if (!skillGap.value?.hasJobSkills) return 0;

    const value = Number(skillGap.value.matchPercentage) || 0;
    return Math.min(Math.max(value, 0), 100);
});

const loadSkillGap = async () => {
    loading.value = true;
    error.value = "";

    try {
        skillGap.value = await getSkillGap(route.params.id);
    } catch {
        error.value = "Skill Gap Analyse konnte nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadSkillGap);
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Skill Gap Analyse</h2>

        <div v-if="loading" class="alert alert-info">
            Analyse wird geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else-if="skillGap">
            <div v-if="!skillGap.hasJobSkills" class="alert alert-warning">
                Für diesen Job wurden noch keine Skills hinterlegt.
                Bitte füge zuerst Skills für diesen Job hinzu.
            </div>

            <div class="card mb-4 shadow-sm">
                <div class="card-body">
                    <h4>{{ skillGap.jobTitle || "Unbekannter Job" }}</h4>

                    <p class="mb-2">Match Score</p>

                    <div class="progress mb-3" style="height: 30px">
                        <div class="progress-bar" role="progressbar" :style="`width: ${matchPercentage}%`">
                            {{ skillGap.hasJobSkills ? `${matchPercentage}%` : "Keine Skills" }}
                        </div>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-4">
                    <div class="card shadow-sm mb-3 h-100">
                        <div class="card-body">
                            <h5>Job Skills</h5>

                            <ul v-if="hasJobSkills" class="list-group">
                                <li v-for="skill in skillGap.jobSkills" :key="skill" class="list-group-item">
                                    {{ skill }}
                                </li>
                            </ul>

                            <p v-else class="text-muted mt-3">
                                Für diesen Job wurden noch keine Skills hinterlegt.
                            </p>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm mb-3 h-100">
                        <div class="card-body">
                            <h5>Passende Skills</h5>

                            <ul v-if="hasMatchedSkills" class="list-group">
                                <li v-for="skill in skillGap.matchedSkills" :key="skill"
                                    class="list-group-item text-success">
                                    ✓ {{ skill }}
                                </li>
                            </ul>

                            <p v-else-if="skillGap.hasJobSkills" class="text-muted mt-3">
                                Keine passenden Skills gefunden.
                            </p>

                            <p v-else class="text-muted mt-3">
                                Noch keine Auswertung möglich.
                            </p>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm mb-3 h-100">
                        <div class="card-body">
                            <h5>Fehlende Skills</h5>

                            <ul v-if="hasMissingSkills" class="list-group">
                                <li v-for="skill in skillGap.missingSkills" :key="skill"
                                    class="list-group-item text-danger">
                                    ✗ {{ skill }}
                                </li>
                            </ul>

                            <p v-else-if="skillGap.hasJobSkills" class="text-success mt-3">
                                Du erfüllst alle Skill-Anforderungen.
                            </p>

                            <p v-else class="text-muted mt-3">
                                Keine fehlenden Skills berechenbar.
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            <div class="card shadow-sm mb-3">
                <div class="card-body">
                    <h5>Empfohlene Kurse</h5>

                    <ul v-if="hasRecommendedCourses" class="list-group">
                        <li v-for="course in skillGap.recommendedCourses" :key="course.id" class="list-group-item">
                            <router-link :to="`/courses/${course.id}`">
                                {{ course.title || "Unbekannter Kurs" }}
                            </router-link>
                        </li>
                    </ul>

                    <BaseEmptyState v-if="!hasRecommendedCourses" message="Keine Kursempfehlungen gefunden." />
                </div>
            </div>

            <router-link :to="`/jobs/${skillGap.jobId}`" class="btn btn-secondary mt-3">
                Zurück zum Job
            </router-link>
        </template>
    </div>
</template>