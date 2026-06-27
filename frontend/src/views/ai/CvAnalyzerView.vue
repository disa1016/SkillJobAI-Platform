<script setup>
import { computed, ref } from "vue";
import api from "../../services/api";

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
        const { data } = await api.post("/ai/analyze-cv", {
            cvText: cvText.value,
        });

        result.value = data;
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
        const formData = new FormData();
        formData.append("file", selectedFile.value);

        const { data } = await api.post("/ai/analyze-cv-pdf", formData, {
            headers: {
                "Content-Type": "multipart/form-data",
            },
        });

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
    <div class="container mt-4">
        <h1 class="mb-4">AI CV Analyzer</h1>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div class="row">
            <div class="col-md-6 mb-4">
                <div class="card shadow-sm h-100">
                    <div class="card-body">
                        <h5>Text analysieren</h5>

                        <label class="form-label">Lebenslauf-Text</label>

                        <textarea v-model="cvText" class="form-control mb-3" rows="8"
                            placeholder="Füge hier deinen Lebenslauf-Text ein..." />

                        <button type="button" class="btn btn-primary" :disabled="!canAnalyzeText" @click="analyzeCv">
                            {{ loading ? "Analysiere..." : "CV analysieren" }}
                        </button>
                    </div>
                </div>
            </div>

            <div class="col-md-6 mb-4">
                <div class="card shadow-sm h-100">
                    <div class="card-body">
                        <h5>PDF hochladen</h5>

                        <label class="form-label">Lebenslauf als PDF</label>

                        <input type="file" class="form-control mb-3" accept="application/pdf"
                            @change="handleFileChange" />

                        <button type="button" class="btn btn-success" :disabled="!canAnalyzePdf" @click="analyzePdf">
                            {{ loading ? "Analysiere..." : "PDF analysieren" }}
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <div v-if="result" class="card shadow-sm mb-4">
            <div class="card-body">
                <h4>Analyse Ergebnis</h4>

                <p class="display-6 text-primary">
                    Score: {{ result.score ?? 0 }}/100
                </p>

                <h5>Skill Kategorien</h5>

                <div v-if="skillCategories.length > 0" class="row mb-4">
                    <div v-for="category in skillCategories" :key="category.name" class="col-md-6 mb-3">
                        <div class="card h-100 border-0 bg-light">
                            <div class="card-body">
                                <h6 class="fw-bold">
                                    {{ category.name }}
                                </h6>

                                <div class="mb-2">
                                    <strong>Gefunden</strong>

                                    <ul v-if="category.matchedSkills?.length" class="list-group mt-2">
                                        <li v-for="skill in category.matchedSkills" :key="skill"
                                            class="list-group-item list-group-item-success">
                                            {{ skill }}
                                        </li>
                                    </ul>

                                    <p v-else class="text-muted mt-2">
                                        Keine Skills gefunden.
                                    </p>
                                </div>

                                <div>
                                    <strong>Fehlt noch</strong>

                                    <ul v-if="category.missingSkills?.length" class="list-group mt-2">
                                        <li v-for="skill in category.missingSkills" :key="skill"
                                            class="list-group-item list-group-item-warning">
                                            {{ skill }}
                                        </li>
                                    </ul>

                                    <p v-else class="text-success mt-2 mb-0">
                                        Keine fehlenden Skills.
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <p v-else class="text-muted">
                    Keine Skill-Kategorien erkannt.
                </p>

                <h5>Alle gefundenen Skills</h5>

                <ul v-if="skills.length > 0" class="list-group mb-3">
                    <li v-for="skill in skills" :key="skill" class="list-group-item">
                        {{ skill }}
                    </li>
                </ul>

                <p v-else class="text-muted">
                    Keine technischen Skills erkannt.
                </p>

                <h5>Empfehlungen</h5>

                <ul v-if="suggestions.length > 0" class="list-group">
                    <li v-for="suggestion in suggestions" :key="suggestion" class="list-group-item">
                        {{ suggestion }}
                    </li>
                </ul>

                <p v-else class="text-muted">
                    Keine Empfehlungen vorhanden.
                </p>
            </div>
        </div>

        <div v-if="extractedText" class="card shadow-sm">
            <div class="card-body">
                <h5>Aus PDF gelesener Text</h5>

                <pre class="bg-light rounded p-3 mb-0">{{ extractedText }}</pre>
            </div>
        </div>
    </div>
</template>