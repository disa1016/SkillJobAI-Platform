<script setup>
import { ref } from "vue";
import api from "../services/api";

const cvText = ref("");
const recommendations = ref([]);
const loading = ref(false);
const error = ref("");

const getRecommendations = async () => {
  error.value = "";
  recommendations.value = [];
  loading.value = true;

  try {
    const response = await api.post("/ai/job-recommendations", {
      cvText: cvText.value,
    });

    recommendations.value = response.data;
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
        <label class="form-label">
          Lebenslauf-Text
        </label>

        <textarea
          v-model="cvText"
          class="form-control mb-3"
          rows="8"
          placeholder="Füge hier deinen Lebenslauf ein..."
        ></textarea>

        <button
          class="btn btn-primary"
          @click="getRecommendations"
          :disabled="loading || !cvText"
        >
          {{ loading ? "Analysiere..." : "Jobs finden" }}
        </button>
      </div>
    </div>

    <div
      v-if="recommendations.length > 0"
      class="row"
    >
      <div
        v-for="job in recommendations"
        :key="job.jobId"
        class="col-md-6 mb-3"
      >
        <div class="card shadow-sm h-100">
          <div class="card-body">

            <h5>
              {{ job.title }}
            </h5>

            <p class="text-muted">
              {{ job.company }}
            </p>

            <span class="badge bg-success mb-3">
              Match: {{ job.matchScore }}%
            </span>

            <p>
              {{ job.description }}
            </p>

            <h6>Gefundene Skills</h6>

            <ul>
              <li
                v-for="skill in job.matchedSkills"
                :key="skill"
              >
                ✅ {{ skill }}
              </li>
            </ul>

            <h6>Fehlende Skills</h6>

            <ul>
              <li
                v-for="skill in job.missingSkills"
                :key="skill"
              >
                ❌ {{ skill }}
              </li>
            </ul>

            <div class="alert alert-info mt-3">
              {{ job.recommendation }}
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>