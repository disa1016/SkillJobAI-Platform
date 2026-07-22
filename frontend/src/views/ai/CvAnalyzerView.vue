<script setup>
import { computed, ref } from "vue";
import {
    analyzeCv as analyzeCvRequest,
    analyzeCvPdf,
} from "@/services/aiService";
import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

const cvText = ref("");
const selectedFile = ref(null);

const result = ref(null);
const extractedText = ref("");

const loading = ref(false);
const error = ref("");

const canAnalyzeText = computed(() => {
    return cvText.value.trim() && !loading.value;
});

const canAnalyzePdf = computed(() => {
    return selectedFile.value && !loading.value;
});

const skillCategories = computed(() => result.value?.skillCategories ?? []);
const skills = computed(() => result.value?.skills ?? []);
const suggestions = computed(() => result.value?.suggestions ?? []);

const clearResult = () => {
    result.value = null;
    extractedText.value = "";
};

const clearMessages = () => {
    error.value = "";
    clearResult();
};

const getErrorMessage = (err, fallback) => {
    return err.response?.data?.message || fallback;
};

const normalizePdfResult = (data) => {
    return {
        score: data.score,
        skills: data.skills ?? [],
        skillCategories: data.skillCategories ?? [],
        suggestions: data.suggestions ?? [],
    };
};

const analyzeCv = async () => {
    loading.value = true;
    clearMessages();

    try {
        result.value = await analyzeCvRequest(cvText.value);
    } catch {
        error.value = "CV konnte nicht analysiert werden.";
    } finally {
        loading.value = false;
    }
};

const handleFileChange = (event) => {
    error.value = "";

    const file = event.target.files?.[0];

    if (!file) {
        selectedFile.value = null;
        return;
    }

    if (file.type !== "application/pdf") {
        selectedFile.value = null;
        event.target.value = "";
        error.value = "Bitte nur PDF-Dateien hochladen.";
        return;
    }

    selectedFile.value = file;
};

const analyzePdf = async () => {
    if (!selectedFile.value) return;

    loading.value = true;
    clearMessages();

    try {
        const data = await analyzeCvPdf(selectedFile.value);

        result.value = normalizePdfResult(data);
        extractedText.value = data.extractedText || "";
    } catch (err) {
        error.value = getErrorMessage(err, "PDF konnte nicht analysiert werden.");
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <main class="container py-4">
        <PageHeader title="AI CV Analyzer"
            description="Analysiere deinen Lebenslauf als Text oder PDF und erhalte Hinweise zu Skills und Verbesserungsmöglichkeiten." />
        <BaseAlert v-if="error" type="danger" :message="error" />

        <div class="row g-4 mb-4">
            <div class="col-12 col-lg-6">
                <div class="card border-0 shadow-sm h-100">
                    <div class="card-header bg-body border-bottom">
                        <h2 class="h5 mb-0">Text analysieren</h2>
                    </div>
                    <div class="card-body d-flex flex-column">
                        <label for="cv-text" class="form-label">Lebenslauf-Text</label>
                        <textarea id="cv-text" v-model="cvText" class="form-control flex-grow-1" rows="9"
                            placeholder="Füge hier deinen Lebenslauf-Text ein..."></textarea>
                        <div class="d-grid d-sm-block mt-3"><button type="button" class="btn btn-primary"
                                :disabled="!canAnalyzeText" @click="analyzeCv"><span v-if="loading"
                                    class="spinner-border spinner-border-sm me-2"></span>{{ loading ? "Wird analysiert..." : "CV analysieren" }}</button></div>
                    </div>
                </div>
            </div>
            <div class="col-12 col-lg-6">
                <div class="card border-0 shadow-sm h-100">
                    <div class="card-header bg-body border-bottom">
                        <h2 class="h5 mb-0">PDF analysieren</h2>
                    </div>
                    <div class="card-body d-flex flex-column">
                        <label for="cv-pdf" class="form-label">Lebenslauf als PDF</label>
                        <input id="cv-pdf" type="file" class="form-control" accept="application/pdf"
                            @change="handleFileChange" />
                        <div class="form-text">Es werden ausschließlich PDF-Dateien akzeptiert.</div>
                        <div class="d-grid d-sm-block mt-auto pt-3"><button type="button"
                                class="btn btn-outline-primary" :disabled="!canAnalyzePdf" @click="analyzePdf"><span
                                    v-if="loading" class="spinner-border spinner-border-sm me-2"></span>{{ loading ?
                                        "Wird analysiert..." : "PDF analysieren" }}</button></div>
                    </div>
                </div>
            </div>
        </div>

        <div v-if="result" class="card border-0 shadow-sm mb-4">
            <div
                class="card-header bg-body border-bottom d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-2">
                <h2 class="h5 mb-0">Analyse-Ergebnis</h2><span class="badge text-bg-primary fs-6">Score: {{ result.score
                    ?? 0 }}/100</span>
            </div>
            <div class="card-body">
                <h3 class="h6 mb-3">Skill-Kategorien</h3>
                <div v-if="skillCategories.length" class="row g-3 mb-4">
                    <div v-for="category in skillCategories" :key="category.name" class="col-12 col-xl-6">
                        <div class="card bg-body-tertiary border-0 h-100">
                            <div class="card-body">
                                <h4 class="h6">{{ category.name }}</h4>
                                <p class="small fw-semibold mb-2">Gefunden</p>
                                <div v-if="category.matchedSkills?.length" class="d-flex flex-wrap gap-2 mb-3"><span
                                        v-for="skill in category.matchedSkills" :key="skill"
                                        class="badge text-bg-success">{{ skill }}</span></div>
                                <p v-else class="text-body-secondary small">Keine Skills gefunden.</p>
                                <p class="small fw-semibold mb-2">Fehlt noch</p>
                                <div v-if="category.missingSkills?.length" class="d-flex flex-wrap gap-2"><span
                                        v-for="skill in category.missingSkills" :key="skill"
                                        class="badge text-bg-warning">{{ skill }}</span></div>
                                <p v-else class="text-success small mb-0">Keine fehlenden Skills.</p>
                            </div>
                        </div>
                    </div>
                </div>
                <BaseEmptyState v-else title="Keine Kategorien erkannt"
                    message="In der Analyse wurden keine Skill-Kategorien gefunden." />

                <div class="row g-4">
                    <div class="col-12 col-lg-6">
                        <h3 class="h6">Alle gefundenen Skills</h3>
                        <ul v-if="skills.length" class="list-group list-group-flush border rounded">
                            <li v-for="skill in skills" :key="skill" class="list-group-item">{{ skill }}</li>
                        </ul>
                        <p v-else class="text-body-secondary mb-0">Keine technischen Skills erkannt.</p>
                    </div>
                    <div class="col-12 col-lg-6">
                        <h3 class="h6">Empfehlungen</h3>
                        <ul v-if="suggestions.length" class="list-group list-group-flush border rounded">
                            <li v-for="suggestion in suggestions" :key="suggestion" class="list-group-item">{{
                                suggestion }}</li>
                        </ul>
                        <p v-else class="text-body-secondary mb-0">Keine Empfehlungen vorhanden.</p>
                    </div>
                </div>
            </div>
        </div>

        <div v-if="extractedText" class="card border-0 shadow-sm">
            <div class="card-header bg-body border-bottom">
                <h2 class="h5 mb-0">Aus PDF gelesener Text</h2>
            </div>
            <div class="card-body">
                <pre class="bg-body-tertiary border rounded p-3 mb-0 text-wrap">{{ extractedText }}</pre>
            </div>
        </div>
    </main>
</template>
