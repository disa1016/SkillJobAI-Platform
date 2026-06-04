<script setup>
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import api from "../services/api";

const route = useRoute();

const job = ref(null);
const coverLetter = ref("");
const cvSummary = ref("");

const loading = ref(true);
const generating = ref(false);
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

const generateCoverLetter = async () => {
  error.value = "";
  success.value = "";
  generating.value = true;

  try {
    const response = await api.post("/ai/generate-cover-letter", {
      fullName: user?.fullName || "",
      company: job.value.company,
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

  try {
    await api.post("/applications", {
      jobId: job.value.id,
      coverLetter: coverLetter.value,
    });

    success.value = "Bewerbung wurde erfolgreich gesendet.";
    coverLetter.value = "";
    cvSummary.value = "";
  } catch (err) {
    if (err.response?.data?.message) {
      error.value = err.response.data.message;
    } else {
      error.value = "Bewerbung konnte nicht gesendet werden.";
    }
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

        <p v-if="job.company">
          <strong>{{ job.company.name }}</strong> · {{ job.company.location }}
        </p>
        <span class="badge bg-success mb-3">
          {{ job.salary }}
        </span>

        <p>{{ job.description }}</p>
        <router-link
  :to="`/jobs/${job.id}/skill-gap`"
  class="btn btn-warning mb-3"
>
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

        <textarea v-model="coverLetter" class="form-control mb-3" rows="8"
          placeholder="Schreibe dein Anschreiben oder generiere es mit AI..."></textarea>

        <button class="btn btn-primary" @click="applyToJob" :disabled="!coverLetter">
          Bewerbung senden
        </button>
      </div>
    </div>
  </div>
</template>