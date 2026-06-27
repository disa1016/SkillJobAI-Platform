<script setup>
import { computed, ref } from "vue";
import api from "../../services/api";

const cvText = ref("");
const jobDescription = ref("");

const result = ref(null);
const loading = ref(false);
const error = ref("");

const matchedSkills = computed(() => result.value?.matchedSkills ?? []);
const missingSkills = computed(() => result.value?.missingSkills ?? []);

const canAnalyze = computed(() => {
    return (
        cvText.value.trim() &&
        jobDescription.value.trim() &&
        !loading.value
    );
});

const matchScore = computed(() => {
    const value = Number(result.value?.matchScore) || 0;
    return Math.min(Math.max(value, 0), 100);
});

const clearMessages = () => {
    error.value = "";
    result.value = null;
};

const analyzeMatch = async () => {
    loading.value = true;
    clearMessages();

    try {
        const { data } = await api.post("/ai/job-match", {
            cvText: cvText.value,
            jobDescription: jobDescription.value,
        });

        result.value = data;
    } catch {
        error.value = "Job Matching konnte nicht durchgeführt werden.";
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <div class="container mt-4">
        <h1 class="mb-4">AI Job Matcher</h1>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div class="card shadow-sm mb-4">
            <div class="card-body">
                <form @submit.prevent="analyzeMatch">
                    <div class="mb-3">
                        <label class="form-label">Lebenslauf-Text</label>

                        <textarea v-model="cvText" class="form-control" rows="6"
                            placeholder="Füge hier deinen CV-Text ein..." required />
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Jobbeschreibung</label>

                        <textarea v-model="jobDescription" class="form-control" rows="6"
                            placeholder="Füge hier die Jobbeschreibung ein..." required />
                    </div>

                    <button type="submit" class="btn btn-primary" :disabled="!canAnalyze">
                        {{ loading ? "Analysiere..." : "Match berechnen" }}
                    </button>
                </form>
            </div>
        </div>

        <div v-if="result" class="card shadow-sm">
            <div class="card-body">
                <h4>Matching Ergebnis</h4>

                <p class="display-6 text-primary">
                    Match Score: {{ matchScore }}%
                </p>

                <h5>Gefundene Skills</h5>

                <ul v-if="matchedSkills.length > 0" class="list-group mb-3">
                    <li v-for="skill in matchedSkills" :key="skill" class="list-group-item">
                        {{ skill }}
                    </li>
                </ul>

                <p v-else class="text-muted">
                    Keine passenden Skills gefunden.
                </p>

                <h5>Fehlende Skills</h5>

                <ul v-if="missingSkills.length > 0" class="list-group mb-3">
                    <li v-for="skill in missingSkills" :key="skill" class="list-group-item">
                        {{ skill }}
                    </li>
                </ul>

                <p v-else class="text-success">
                    Keine fehlenden Skills gefunden.
                </p>

                <div v-if="result.recommendation" class="alert alert-info">
                    {{ result.recommendation }}
                </div>
            </div>
        </div>
    </div>
</template>