<script setup>
import { computed, ref } from "vue";
import { analyzeJobMatch } from "@/services/aiService";
import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

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
        result.value = await analyzeJobMatch({
            cvText: cvText.value,
            jobDescription: jobDescription.value,
        });
    } catch {
        error.value = "Job Matching konnte nicht durchgeführt werden.";
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <main class="container py-4">
        <PageHeader title="AI Job Matcher"
            description="Vergleiche deinen Lebenslauf mit einer Stellenbeschreibung und erkenne passende sowie fehlende Skills." />
        <BaseAlert v-if="error" type="danger" :message="error" />

        <div class="card border-0 shadow-sm mb-4">
            <div class="card-body p-4">
                <form @submit.prevent="analyzeMatch">
                    <div class="row g-3">
                        <div class="col-12 col-lg-6">
                            <label for="match-cv" class="form-label">Lebenslauf-Text</label>
                            <textarea id="match-cv" v-model="cvText" class="form-control" rows="9"
                                placeholder="Füge hier deinen CV-Text ein..." required></textarea>
                        </div>
                        <div class="col-12 col-lg-6">
                            <label for="match-job" class="form-label">Jobbeschreibung</label>
                            <textarea id="match-job" v-model="jobDescription" class="form-control" rows="9"
                                placeholder="Füge hier die Jobbeschreibung ein..." required></textarea>
                        </div>
                    </div>
                    <div class="d-grid d-sm-block mt-4">
                        <button type="submit" class="btn btn-primary" :disabled="!canAnalyze">
                            <span v-if="loading" class="spinner-border spinner-border-sm me-2"
                                aria-hidden="true"></span>{{ loading ? "Wird analysiert..." : "Match berechnen" }}
                        </button>
                    </div>
                </form>
            </div>
        </div>

        <div v-if="result" class="card border-0 shadow-sm">
            <div
                class="card-header bg-body border-bottom d-flex flex-column flex-sm-row justify-content-between gap-2 align-items-sm-center">
                <h2 class="h5 mb-0">Matching-Ergebnis</h2><span class="badge text-bg-primary fs-6">{{ matchScore }} %
                    Match</span>
            </div>
            <div class="card-body">
                <div class="row g-4">
                    <div class="col-12 col-lg-6">
                        <h3 class="h6">Gefundene Skills</h3>
                        <ul v-if="matchedSkills.length" class="list-group list-group-flush border rounded">
                            <li v-for="skill in matchedSkills" :key="skill" class="list-group-item"><i
                                    class="bi bi-check-circle text-success me-2"></i>{{ skill }}</li>
                        </ul>
                        <BaseEmptyState v-else title="Keine Treffer"
                            message="Es wurden keine passenden Skills gefunden." />
                    </div>
                    <div class="col-12 col-lg-6">
                        <h3 class="h6">Fehlende Skills</h3>
                        <ul v-if="missingSkills.length" class="list-group list-group-flush border rounded">
                            <li v-for="skill in missingSkills" :key="skill" class="list-group-item"><i
                                    class="bi bi-exclamation-circle text-warning me-2"></i>{{ skill }}</li>
                        </ul>
                        <BaseAlert v-else type="success" message="Keine fehlenden Skills gefunden." />
                    </div>
                </div>
                <BaseAlert v-if="result.recommendation" class="mt-4 mb-0" type="info"
                    :message="result.recommendation" />
            </div>
        </div>
    </main>
</template>
