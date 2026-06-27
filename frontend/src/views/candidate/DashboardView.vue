<script setup>
import { onMounted, ref } from "vue";
import api from "../../services/api";

const user = JSON.parse(localStorage.getItem("user") || "null");

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

const getMatchClass = (score) => {
  if (score >= 70) return "bg-success";
  if (score >= 40) return "bg-warning text-dark";
  return "bg-danger";
};

const loadDashboard = async () => {
  loading.value = true;
  error.value = "";

  try {
    const response = await api.get("/candidate/dashboard");
    dashboard.value = response.data;
  } catch {
    error.value = "Dashboard-Daten konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};

onMounted(loadDashboard);
</script>

<template>
  <div class="container mt-4">
    <div class="mb-4">
      <h1 class="mb-2">
        Willkommen {{ user?.fullName }}
      </h1>

      <p class="text-muted mb-0">
        Rolle: {{ user?.role }}
      </p>
    </div>

    <div v-if="loading" class="alert alert-info">
      Dashboard wird geladen...
    </div>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-if="dashboard">
      <div class="row g-3">
        <div class="col-md-4">
          <div class="card shadow-sm border-primary h-100">
            <div class="card-body">
              <h5>Meine Bewerbungen</h5>

              <p class="display-5 text-primary mb-0">
                {{ dashboard.applicationsCount }}
              </p>
            </div>
          </div>
        </div>

        <div class="col-md-4">
          <div class="card shadow-sm border-success h-100">
            <div class="card-body">
              <h5>Meine Kurse</h5>

              <p class="display-5 text-success mb-0">
                {{ dashboard.enrollmentsCount }}
              </p>
            </div>
          </div>
        </div>

        <div class="col-md-4">
          <div class="card shadow-sm border-warning h-100">
            <div class="card-body">
              <h5>Abgeschlossene Lektionen</h5>

              <p class="display-5 text-warning mb-0">
                {{ dashboard.completedLessonsCount }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div class="card shadow-sm mt-4">
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h4 class="mb-0">Meine Skills</h4>

            <router-link to="/profile/skills" class="btn btn-outline-primary btn-sm">
              Skills bearbeiten
            </router-link>
          </div>

          <div v-if="dashboard.userSkills?.length">
            <span v-for="skill in dashboard.userSkills" :key="skill" class="badge bg-success me-2 mb-2">
              {{ skill }}
            </span>
          </div>

          <p v-else class="text-muted mb-0">
            Du hast noch keine Skills hinterlegt.
          </p>
        </div>
      </div>

      <div v-if="dashboard.missingSkills?.length" class="card shadow-sm mt-4">
        <div class="card-body">
          <h4>Empfohlene Skills</h4>

          <div class="mt-3">
            <span v-for="skill in dashboard.missingSkills" :key="skill" class="badge bg-danger me-2 mb-2">
              ✗ {{ skill }}
            </span>
          </div>
        </div>
      </div>

      <div v-if="dashboard.recommendedCourses?.length" class="card shadow-sm mt-4">
        <div class="card-body">
          <h4>Empfohlene Kurse</h4>

          <div class="mt-3">
            <router-link v-for="course in dashboard.recommendedCourses" :key="course.id" :to="`/courses/${course.id}`"
              class="badge bg-primary me-2 mb-2 text-decoration-none">
              {{ course.title }}
            </router-link>
          </div>
        </div>
      </div>

      <div v-if="dashboard.topJobMatches?.length" class="card shadow-sm mt-4">
        <div class="card-body">
          <h4>Top Job Matches</h4>

          <div v-for="job in dashboard.topJobMatches" :key="job.id" class="border rounded p-3 mb-3">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <div>
                <h5 class="mb-1">
                  {{ job.title }}
                </h5>

                <p class="text-muted mb-0">
                  {{ job.company?.name || "Keine Firma" }}
                </p>
              </div>

              <span class="badge" :class="getMatchClass(job.matchPercentage)">
                {{ job.matchPercentage }}%
              </span>
            </div>

            <router-link :to="`/jobs/${job.id}`" class="btn btn-outline-primary btn-sm">
              Job ansehen
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>