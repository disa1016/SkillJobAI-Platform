<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import api from "../../services/api";;

const MAX_FILE_SIZE = 5 * 1024 * 1024;

const route = useRoute();

const job = ref(null);
const coverLetter = ref("");
const cvSummary = ref("");

const cvFile = ref(null);
const certificateFile = ref(null);
const portfolioFile = ref(null);

const loading = ref(true);
const generating = ref(false);
const submitting = ref(false);

const error = ref("");
const success = ref("");

const user = JSON.parse(localStorage.getItem("user")) || null;

const fileFields = computed(() => [
  {
    label: "Lebenslauf / CV als PDF",
    handler: handleCvFile,
  },
  {
    label: "Zeugnis / Zertifikat als PDF",
    handler: handleCertificateFile,
  },
  {
    label: "Portfolio als PDF",
    handler: handlePortfolioFile,
  },
]);

const canGenerateCoverLetter = computed(() => {
  return Boolean(cvSummary.value.trim()) && !generating.value;
});

const canSubmitApplication = computed(() => {
  return Boolean(coverLetter.value.trim()) && !submitting.value;
});

const loadJob = async () => {
  loading.value = true;
  error.value = "";

  try {
    const { data } = await api.get(`/jobs/${route.params.id}`);
    job.value = data;
  } catch {
    error.value = "Job konnte nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};

const clearMessages = () => {
  error.value = "";
  success.value = "";
};

const validatePdf = (file) => {
  if (!file) return true;

  if (file.type !== "application/pdf") {
    error.value = "Bitte nur PDF-Dateien hochladen.";
    return false;
  }

  if (file.size > MAX_FILE_SIZE) {
    error.value = "PDF-Dateien dürfen maximal 5MB groß sein.";
    return false;
  }

  return true;
};

const handleFileChange = (event, targetRef) => {
  clearMessages();

  const file = event.target.files?.[0];

  if (!validatePdf(file)) {
    targetRef.value = null;
    event.target.value = "";
    return;
  }

  targetRef.value = file || null;
};

const handleCvFile = (event) => {
  handleFileChange(event, cvFile);
};

const handleCertificateFile = (event) => {
  handleFileChange(event, certificateFile);
};

const handlePortfolioFile = (event) => {
  handleFileChange(event, portfolioFile);
};

const resetApplicationForm = () => {
  coverLetter.value = "";
  cvSummary.value = "";
  cvFile.value = null;
  certificateFile.value = null;
  portfolioFile.value = null;

  document.querySelectorAll("input[type='file']").forEach((input) => {
    input.value = "";
  });
};

const generateCoverLetter = async () => {
  clearMessages();
  generating.value = true;

  try {
    const { data } = await api.post("/ai/generate-cover-letter", {
      fullName: user?.fullName || "",
      company: job.value?.company?.name || "das Unternehmen",
      jobTitle: job.value?.title || "",
      cvSummary: cvSummary.value,
    });

    coverLetter.value = data.coverLetter;
    success.value = "Anschreiben wurde generiert.";
  } catch {
    error.value = "Anschreiben konnte nicht generiert werden.";
  } finally {
    generating.value = false;
  }
};

const applyToJob = async () => {
  clearMessages();
  submitting.value = true;

  try {
    const formData = new FormData();

    formData.append("jobId", job.value.id);
    formData.append("coverLetter", coverLetter.value);

    if (cvFile.value) {
      formData.append("cvFile", cvFile.value);
    }

    if (certificateFile.value) {
      formData.append("certificateFile", certificateFile.value);
    }

    if (portfolioFile.value) {
      formData.append("portfolioFile", portfolioFile.value);
    }

    await api.post("/applications", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    success.value = "Bewerbung wurde erfolgreich gesendet.";
    resetApplicationForm();
  } catch (err) {
    error.value =
      err.response?.data?.message || "Bewerbung konnte nicht gesendet werden.";
  } finally {
    submitting.value = false;
  }
};

onMounted(loadJob);
</script>

<template>
  <div class="container mt-4">
    <div v-if="loading" class="alert alert-info">
      Job wird geladen...
    </div>

    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <template v-else-if="job">
      <div v-if="success" class="alert alert-success">
        {{ success }}
      </div>

      <div class="card shadow">
        <div class="card-body">
          <h1>{{ job.title || "Ohne Titel" }}</h1>

          <p v-if="job.company" class="text-muted mb-2">
            Firma:
            <router-link :to="`/companies/${job.company.id}`" class="text-decoration-none fw-semibold">
              {{ job.company.name }}
            </router-link>

            <span v-if="job.company.location">
              · {{ job.company.location }}
            </span>
          </p>

          <span class="badge bg-success mb-3">
            {{ job.salary || "Kein Gehalt angegeben" }}
          </span>

          <p>
            {{ job.description || "Keine Beschreibung vorhanden." }}
          </p>

          <router-link :to="`/jobs/${job.id}/skill-gap`" class="btn btn-warning mb-3">
            Skill Gap Analyse
          </router-link>

          <hr />

          <h4>Bewerben</h4>

          <div class="mb-3">
            <label class="form-label">
              CV Zusammenfassung für AI Anschreiben
            </label>

            <textarea v-model="cvSummary" class="form-control" rows="4"
              placeholder="z.B. Ich habe Erfahrung mit C#, ASP.NET Core, Vue.js, PostgreSQL und GitHub." />
          </div>

          <button type="button" class="btn btn-outline-primary mb-3" :disabled="!canGenerateCoverLetter"
            @click="generateCoverLetter">
            {{ generating ? "Generiere..." : "AI Anschreiben generieren" }}
          </button>

          <div class="mb-3">
            <label class="form-label">
              Anschreiben
            </label>

            <textarea v-model="coverLetter" class="form-control" rows="8"
              placeholder="Schreibe dein Anschreiben oder generiere es mit AI..." />
          </div>

          <div class="card bg-light border-0 mb-3">
            <div class="card-body">
              <h5 class="mb-3">Bewerbungsunterlagen</h5>

              <div v-for="field in fileFields" :key="field.label" class="mb-3">
                <label class="form-label">
                  {{ field.label }}
                </label>

                <input type="file" accept="application/pdf" class="form-control" @change="field.handler" />
              </div>

              <small class="text-muted">
                Erlaubt sind nur PDF-Dateien bis maximal 5MB.
              </small>
            </div>
          </div>

          <button type="button" class="btn btn-primary" :disabled="!canSubmitApplication" @click="applyToJob">
            {{ submitting ? "Bewerbung wird gesendet..." : "Bewerbung senden" }}
          </button>
        </div>
      </div>
    </template>
  </div>
</template>