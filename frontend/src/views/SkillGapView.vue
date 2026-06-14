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
            <div v-if="!skillGap.hasJobSkills" class="alert alert-warning">
                Für diesen Job wurden noch keine Skills hinterlegt.
                Bitte füge zuerst Skills für diesen Job hinzu.
            </div>

            <div class="card mb-4 shadow-sm">
                <div class="card-body">
                    <h4>{{ skillGap.jobTitle }}</h4>

                    <p class="mb-2">Match Score</p>

                    <div class="progress mb-3" style="height: 30px;">
                        <div class="progress-bar" role="progressbar"
                            :style="`width: ${skillGap.hasJobSkills ? skillGap.matchPercentage : 0}%`">
                            {{ skillGap.hasJobSkills ? skillGap.matchPercentage + "%" : "Keine Skills" }}
                        </div>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-4">
                    <div class="card shadow-sm mb-3">
                        <div class="card-body">
                            <h5>Job Skills</h5>

                            <ul v-if="skillGap.jobSkills && skillGap.jobSkills.length > 0" class="list-group">
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
                    <div class="card shadow-sm mb-3">
                        <div class="card-body">
                            <h5>Passende Skills</h5>

                            <ul v-if="skillGap.matchedSkills && skillGap.matchedSkills.length > 0" class="list-group">
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
                    <div class="card shadow-sm mb-3">
                        <div class="card-body">
                            <h5>Fehlende Skills</h5>

                            <ul v-if="skillGap.missingSkills && skillGap.missingSkills.length > 0" class="list-group">
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

                    <ul v-if="skillGap.recommendedCourses && skillGap.recommendedCourses.length > 0" class="list-group">
                        <li v-for="course in skillGap.recommendedCourses" :key="course.id" class="list-group-item">
                            <router-link :to="`/courses/${course.id}`">
                                {{ course.title }}
                            </router-link>
                        </li>
                    </ul>

                    <p v-else class="text-muted mt-3">
                        Keine Kursempfehlungen gefunden.
                    </p>
                </div>
            </div>

            <router-link :to="`/jobs/${skillGap.jobId}`" class="btn btn-secondary mt-3">
                Zurück zum Job
            </router-link>
        </div>
    </div>
</template>