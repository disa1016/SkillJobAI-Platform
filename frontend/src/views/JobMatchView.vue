<script setup>
import { ref } from "vue";
import api from "../services/api";

const cvText = ref("");
const jobDescription = ref("");
const result = ref(null);
const loading = ref(false);
const error = ref("");

const analyzeMatch = async () => {
    error.value = "";
    result.value = null;
    loading.value = true;

    try {
        const response = await api.post("/ai/job-match", {
            cvText: cvText.value,
            jobDescription: jobDescription.value,
        });

        result.value = response.data;
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
                <div class="mb-3">
                    <label class="form-label">Lebenslauf-Text</label>
                    <textarea v-model="cvText" class="form-control" rows="6"
                        placeholder="Füge hier deinen CV-Text ein..."></textarea>
                </div>

                <div class="mb-3">
                    <label class="form-label">Jobbeschreibung</label>
                    <textarea v-model="jobDescription" class="form-control" rows="6"
                        placeholder="Füge hier die Jobbeschreibung ein..."></textarea>
                </div>

                <button class="btn btn-primary" @click="analyzeMatch" :disabled="loading || !cvText || !jobDescription">
                    {{ loading ? "Analysiere..." : "Match berechnen" }}
                </button>
            </div>
        </div>

        <div v-if="result" class="card shadow-sm">
            <div class="card-body">
                <h4>Matching Ergebnis</h4>

                <p class="display-6 text-primary">
                    Match Score: {{ result.matchScore }}%
                </p>

                <h5>Gefundene Skills</h5>
                <ul class="list-group mb-3">
                    <li v-for="skill in result.matchedSkills" :key="skill" class="list-group-item">
                        {{ skill }}
                    </li>
                </ul>

                <h5>Fehlende Skills</h5>
                <ul class="list-group mb-3">
                    <li v-for="skill in result.missingSkills" :key="skill" class="list-group-item">
                        {{ skill }}
                    </li>
                </ul>

                <div class="alert alert-info">
                    {{ result.recommendation }}
                </div>
            </div>
        </div>
    </div>
</template>