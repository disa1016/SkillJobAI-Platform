<script setup>
import { computed, ref } from "vue";
import { generateCoverLetter as generateCoverLetterRequest } from "@/services/aiService";
import BaseAlert from "@/components/shared/BaseAlert.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

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
    const data = await generateCoverLetterRequest({
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
  <main class="container py-4">
    <PageHeader title="AI-Anschreiben" description="Erstelle aus deinen Angaben einen ersten Entwurf für ein individuelles Anschreiben." />
    <BaseAlert v-if="error" type="danger" :message="error" />

    <div class="card border-0 shadow-sm mb-4">
      <div class="card-body p-4">
        <form @submit.prevent="generateCoverLetter">
          <div class="row g-3">
            <div class="col-12 col-md-6">
              <label for="cover-name" class="form-label">Vollständiger Name</label>
              <input id="cover-name" v-model="fullName" type="text" class="form-control" required />
            </div>
            <div class="col-12 col-md-6">
              <label for="cover-company" class="form-label">Firma</label>
              <input id="cover-company" v-model="company" type="text" class="form-control" required />
            </div>
            <div class="col-12">
              <label for="cover-job" class="form-label">Jobtitel</label>
              <input id="cover-job" v-model="jobTitle" type="text" class="form-control" required />
            </div>
            <div class="col-12">
              <label for="cover-summary" class="form-label">CV-Zusammenfassung</label>
              <textarea id="cover-summary" v-model="cvSummary" rows="7" class="form-control" placeholder="z. B. Erfahrung mit C#, ASP.NET Core, Vue.js, PostgreSQL und GitHub." required></textarea>
            </div>
          </div>
          <div class="d-grid d-sm-block mt-4">
            <button type="submit" class="btn btn-primary" :disabled="loading || !canGenerate">
              <span v-if="loading" class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>{{ loading ? "Wird generiert..." : "Anschreiben generieren" }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="coverLetter" class="card border-0 shadow-sm">
      <div class="card-header bg-body border-bottom"><h2 class="h5 mb-0">Generiertes Anschreiben</h2></div>
      <div class="card-body"><pre class="bg-body-tertiary border rounded p-3 mb-0 text-wrap">{{ coverLetter }}</pre></div>
    </div>
  </main>
</template>
