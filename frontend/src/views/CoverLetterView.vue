<script setup>
import { ref } from "vue";
import api from "../services/api";

const fullName = ref("");
const company = ref("");
const jobTitle = ref("");
const cvSummary = ref("");

const coverLetter = ref("");
const loading = ref(false);
const error = ref("");

const generateCoverLetter = async () => {
  error.value = "";
  coverLetter.value = "";
  loading.value = true;

  try {
    const response = await api.post("/ai/generate-cover-letter", {
      fullName: fullName.value,
      company: company.value,
      jobTitle: jobTitle.value,
      cvSummary: cvSummary.value,
    });

    coverLetter.value = response.data.coverLetter;
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
          <label class="form-label">Vollständiger Name</label>
          <input v-model="fullName" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">Firma</label>
          <input v-model="company" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">Jobtitel</label>
          <input v-model="jobTitle" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">CV Zusammenfassung</label>
          <textarea
            v-model="cvSummary"
            class="form-control"
            rows="6"
            placeholder="z.B. Ich habe Erfahrung mit C#, ASP.NET Core, Vue.js, PostgreSQL und GitHub."
          ></textarea>
        </div>

        <button
          class="btn btn-primary"
          @click="generateCoverLetter"
          :disabled="loading || !fullName || !company || !jobTitle || !cvSummary"
        >
          {{ loading ? "Generiere..." : "Anschreiben generieren" }}
        </button>
      </div>
    </div>

    <div v-if="coverLetter" class="card shadow-sm">
      <div class="card-body">
        <h4>Generiertes Anschreiben</h4>

        <pre class="bg-light p-3 rounded mt-3">{{ coverLetter }}</pre>
      </div>
    </div>
  </div>
</template>