<script setup>
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import api from "../services/api";

const route = useRoute();

const skillGap = ref(null);
const loading = ref(true);
const error = ref("");

onMounted(async () => {
    try {
        const jobId = route.params.id;
        const response = await api.get(`/jobs/${jobId}/skill-gap`);
        skillGap.value = response.data;
    } catch {
        error.value = "Skill Gap Analyse konnte nicht geladen werden.";
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Skill Gap Analyse</h2>

        <div v-if="loading" class="alert alert-info">
            Analyse wird geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="skillGap">
            <div class="card mb-4 shadow-sm">
                <div class="card-body">
                    <h4>{{ skillGap.jobTitle }}</h4>

                    <p class="mb-2">Match Score</p>

                    <div class="progress mb-3" style="height: 30px;">
                        <div class="progress-bar" role="progressbar" :style="`width: ${skillGap.matchPercentage}%`">
                            {{ skillGap.matchPercentage }}%
                        </div>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-4">
                    <div class="card shadow-sm mb-3">
                        <div class="card-body">
                            <h5>Meine Skills</h5>
                            <ul class="list-group">
                                <li v-for="skill in skillGap.userSkills" :key="skill" class="list-group-item">
                                    {{ skill }}
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm mb-3">
                        <div class="card-body">
                            <h5>Fehlende Skills</h5>
                            <ul class="list-group">
                                <li v-for="skill in skillGap.missingSkills" :key="skill" class="list-group-item">
                                    {{ skill }}
                                </li>
                            </ul>

                            <p v-if="skillGap.missingSkills.length === 0" class="text-success mt-3">
                                Du erfüllst alle Skill-Anforderungen.
                            </p>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm mb-3">
                        <div class="card-body">
                            <h5>Empfohlene Kurse</h5>

                            <ul class="list-group">
                                <li v-for="course in skillGap.recommendedCourses" :key="course.id"
                                    class="list-group-item">
                                    {{ course.title }}
                                </li>
                            </ul>

                            <p v-if="!skillGap.recommendedCourses || skillGap.recommendedCourses.length === 0"
                                class="text-muted mt-3">
                                Keine Kursempfehlungen gefunden.
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            <router-link :to="`/jobs/${skillGap.jobId}`" class="btn btn-secondary mt-3">
                Zurück zum Job
            </router-link>
        </div>
    </div>
</template>