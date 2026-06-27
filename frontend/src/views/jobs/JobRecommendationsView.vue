<script setup>
import { computed, ref } from "vue";
import api from "../../services/api";

const cvText = ref("");
const recommendations = ref([]);

const loading = ref(false);
const error = ref("");

const hasRecommendations = computed(() => recommendations.value.length > 0);

const canAnalyze = computed(() => {
  return cvText.value.trim() && !loading.value;
});

const getMatchScore = (score) => {
  const value = Number(score) || 0;
  return Math.min(Math.max(value, 0), 100);
};

const clearMessages = () => {
  error.value = "";
  recommendations.value = [];
};

const getRecommendations = async () => {
  loading.value = true;
  clearMessages();

  try {
    const { data } = await api.post("/ai/job-recommendations", {
      cvText: cvText.value,
    });

    recommendations.value = data;
  } catch {
    error.value = "Job Empfehlungen konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="container mt-4">
    <h1 class="mb-4">AI Job Recommendations</h1>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div class="card shadow-sm mb-4">
      <div class="card-body">
        <form @submit.prevent="getRecommendations">
          <label class="form-label">
            Lebenslauf-Text
          </label>

          <textarea v-model="cvText" class="form-control mb-3" rows="8" placeholder="Füge hier deinen Lebenslauf ein..."
            required />

          <button type="submit" class="btn btn-primary" :disabled="!canAnalyze">
            {{ loading ? "Analysiere..." : "Jobs finden" }}
          </button>
        </form>
      </div>
    </div>

    <div v-if="hasRecommendations" class="row">
      <div v-for="job in recommendations" :key="job.jobId" class="col-md-6 mb-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5>
              {{ job.title || "Ohne Titel" }}
            </h5>

            <p class="text-muted">
              {{ job.company || "Keine Firma" }}
            </p>

            <span class="badge bg-success mb-3">
              Match: {{ getMatchScore(job.matchScore) }}%
            </span>

            <p>
              {{ job.description || "Keine Beschreibung vorhanden." }}
            </p>

            <h6>Gefundene Skills</h6>

            <ul v-if="job.matchedSkills?.length">
              <li v-for="skill in job.matchedSkills" :key="skill">
                {{ skill }}
              </li>
            </ul>

            <p v-else class="text-muted">
              Keine passenden Skills gefunden.
            </p>

            <h6>Fehlende Skills</h6>

            <ul v-if="job.missingSkills?.length">
              <li v-for="skill in job.missingSkills" :key="skill">
                {{ skill }}
              </li>
            </ul>

            <p v-else class="text-success">
              Keine fehlenden Skills.
            </p>

            <div v-if="job.recommendation" class="alert alert-info mt-3">
              {{ job.recommendation }}
            </div>

            <router-link v-if="job.jobId" :to="`/jobs/${job.jobId}`" class="btn btn-outline-primary btn-sm mt-2">
              Job ansehen
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>