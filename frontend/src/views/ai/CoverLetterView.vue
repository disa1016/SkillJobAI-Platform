<script setup>
import { computed, ref } from "vue";
import api from "../../services/api";
const fullName = ref("");
const company = ref("");
const jobTitle = ref("");
const cvSummary = ref("");

const coverLetter = ref("");

const loading = ref(false);
const error = ref("");

const canGenerate = computed(() => {
  return (
    fullName.value.trim() &&
    company.value.trim() &&
    jobTitle.value.trim() &&
    cvSummary.value.trim()
  );
});

const clearMessages = () => {
  error.value = "";
  coverLetter.value = "";
};

const generateCoverLetter = async () => {
  loading.value = true;
  clearMessages();

  try {
    const { data } = await api.post("/ai/generate-cover-letter", {
      fullName: fullName.value,
      company: company.value,
      jobTitle: jobTitle.value,
      cvSummary: cvSummary.value,
    });

    coverLetter.value = data.coverLetter;
  } catch {
    error.value = "Anschreiben konnte nicht generiert werden.";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="container mt-4">
    <h1 class="mb-4">AI Cover Letter Generator</h1>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div class="card shadow-sm mb-4">
      <div class="card-body">
        <div class="mb-3">
          <label class="form-label">
            Vollständiger Name
          </label>

          <input v-model="fullName" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">
            Firma
          </label>

          <input v-model="company" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">
            Jobtitel
          </label>

          <input v-model="jobTitle" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">
            CV Zusammenfassung
          </label>

          <textarea v-model="cvSummary" rows="6" class="form-control"
            placeholder="z.B. Ich habe Erfahrung mit C#, ASP.NET Core, Vue.js, PostgreSQL und GitHub." />
        </div>

        <button type="button" class="btn btn-primary" :disabled="loading || !canGenerate" @click="generateCoverLetter">
          {{ loading ? "Generiere..." : "Anschreiben generieren" }}
        </button>
      </div>
    </div>

    <div v-if="coverLetter" class="card shadow-sm">
      <div class="card-body">
        <h4>Generiertes Anschreiben</h4>

        <pre class="bg-light rounded p-3 mt-3 mb-0">{{ coverLetter }}</pre>
      </div>
    </div>
  </div>
</template>