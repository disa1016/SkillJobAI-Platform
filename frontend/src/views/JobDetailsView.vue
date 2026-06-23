<script setup>
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import api from "../services/api";

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

const user = JSON.parse(localStorage.getItem("user"));

onMounted(async () => {
  try {
    const response = await api.get(`/jobs/${route.params.id}`);
    job.value = response.data;
  } catch {
    error.value = "Job konnte nicht geladen werden.";
  } finally {
    loading.value = false;
  }
});

const validatePdf = (file) => {
  if (!file) return true;

  if (file.type !== "application/pdf") {
    error.value = "Bitte nur PDF-Dateien hochladen.";
    return false;
  }

  if (file.size > 5 * 1024 * 1024) {
    error.value = "PDF-Dateien dürfen maximal 5MB groß sein.";
    return false;
  }

  return true;
};

const handleCvFile = (event) => {
  error.value = "";
  const file = event.target.files[0];

  if (!validatePdf(file)) {
    event.target.value = "";
    cvFile.value = null;
    return;
  }

  cvFile.value = file || null;
};

const handleCertificateFile = (event) => {
  error.value = "";
  const file = event.target.files[0];

  if (!validatePdf(file)) {
    event.target.value = "";
    certificateFile.value = null;
    return;
  }

  certificateFile.value = file || null;
};

const handlePortfolioFile = (event) => {
  error.value = "";
  const file = event.target.files[0];

  if (!validatePdf(file)) {
    event.target.value = "";
    portfolioFile.value = null;
    return;
  }

  portfolioFile.value = file || null;
};

const generateCoverLetter = async () => {
  error.value = "";
  success.value = "";
  generating.value = true;

  try {
    const response = await api.post("/ai/generate-cover-letter", {
      fullName: user?.fullName || "",
     company: job.value.company?.name || "das Unternehmen",
      jobTitle: job.value.title,
      cvSummary: cvSummary.value,
    });

    coverLetter.value = response.data.coverLetter;
    success.value = "Anschreiben wurde generiert.";
  } catch {
    error.value = "Anschreiben konnte nicht generiert werden.";
  } finally {
    generating.value = false;
  }
};

const applyToJob = async () => {
  error.value = "";
  success.value = "";
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

    coverLetter.value = "";
    cvSummary.value = "";
    cvFile.value = null;
    certificateFile.value = null;
    portfolioFile.value = null;

    const fileInputs = document.querySelectorAll("input[type='file']");
    fileInputs.forEach((input) => {
      input.value = "";
    });
  } catch (err) {
    if (err.response?.data?.message) {
      error.value = err.response.data.message;
    } else {
      error.value = "Bewerbung konnte nicht gesendet werden.";
    }
  } finally {
    submitting.value = false;
  }
};
</script>

<template>
  <div class="container mt-4">
    <div v-if="loading" class="alert alert-info">
      Job wird geladen...
    </div>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-if="success" class="alert alert-success">
      {{ success }}
    </div>

    <div v-if="job" class="card shadow">
      <div class="card-body">
        <h1>{{ job.title }}</h1>

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
          {{ job.salary }}
        </span>

        <p>{{ job.description }}</p>

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
            placeholder="z.B. Ich habe Erfahrung mit C#, ASP.NET Core, Vue.js, PostgreSQL und GitHub."></textarea>
        </div>

        <button class="btn btn-outline-primary mb-3" @click="generateCoverLetter" :disabled="generating || !cvSummary">
          {{ generating ? "Generiere..." : "AI Anschreiben generieren" }}
        </button>

        <div class="mb-3">
          <label class="form-label">Anschreiben</label>

          <textarea v-model="coverLetter" class="form-control" rows="8"
            placeholder="Schreibe dein Anschreiben oder generiere es mit AI..."></textarea>
        </div>

        <div class="card bg-light border-0 mb-3">
          <div class="card-body">
            <h5 class="mb-3">Bewerbungsunterlagen</h5>

            <div class="mb-3">
              <label class="form-label">
                Lebenslauf / CV als PDF
              </label>

              <input type="file" accept="application/pdf" class="form-control" @change="handleCvFile" />
            </div>

            <div class="mb-3">
              <label class="form-label">
                Zeugnis / Zertifikat als PDF
              </label>

              <input type="file" accept="application/pdf" class="form-control" @change="handleCertificateFile" />
            </div>

            <div class="mb-2">
              <label class="form-label">
                Portfolio als PDF
              </label>

              <input type="file" accept="application/pdf" class="form-control" @change="handlePortfolioFile" />
            </div>

            <small class="text-muted">
              Erlaubt sind nur PDF-Dateien bis maximal 5MB.
            </small>
          </div>
        </div>

        <button class="btn btn-primary" @click="applyToJob" :disabled="!coverLetter || submitting">
          {{ submitting ? "Bewerbung wird gesendet..." : "Bewerbung senden" }}
        </button>
      </div>
    </div>
  </div>
</template>