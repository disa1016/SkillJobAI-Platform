<script setup>
import { computed, onMounted, ref } from "vue";
import { getCandidateDashboard } from "@/services/candidateService";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";

const user = JSON.parse(localStorage.getItem("user") || "null");

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

const stats = computed(() => [
  {
    title: "Meine Bewerbungen",
    value: dashboard.value?.applicationsCount ?? 0,
    cardClass: "border-primary",
    textClass: "text-primary",
  },
  {
    title: "Meine Kurse",
    value: dashboard.value?.enrollmentsCount ?? 0,
    cardClass: "border-success",
    textClass: "text-success",
  },
  {
    title: "Abgeschlossene Lektionen",
    value: dashboard.value?.completedLessonsCount ?? 0,
    cardClass: "border-warning",
    textClass: "text-warning",
  },
]);

const hasUserSkills = computed(() => dashboard.value?.userSkills?.length > 0);
const hasMissingSkills = computed(() => dashboard.value?.missingSkills?.length > 0);
const hasRecommendedCourses = computed(() => dashboard.value?.recommendedCourses?.length > 0);
const hasTopJobMatches = computed(() => dashboard.value?.topJobMatches?.length > 0);

const getMatchClass = (score) => {
  if (score >= 70) return "bg-success";
  if (score >= 40) return "bg-warning text-dark";

  return "bg-danger";
};

const loadDashboard = async () => {
  loading.value = true;
  error.value = "";

  try {
    dashboard.value = await getCandidateDashboard();
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
        Willkommen {{ user?.fullName || "" }}
      </h1>

      <p class="text-muted mb-0">
        Rolle: {{ user?.role || "Unbekannt" }}
      </p>
    </div>

    <BaseSpinner v-if="loading" message="Dashboard wird geladen..." />

    <BaseAlert v-else-if="error" type="danger" :message="error" />

    <template v-else-if="dashboard">
      <div class="row g-3">
        <div v-for="stat in stats" :key="stat.title" class="col-md-4">
          <BaseCard :cardClass="stat.cardClass">
            <h5>{{ stat.title }}</h5>

            <p class="display-5 mb-0" :class="stat.textClass">
              {{ stat.value }}
            </p>
          </BaseCard>
        </div>
      </div>

      <BaseCard title="Meine Skills" class="mt-4">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h4 class="mb-0">Meine Skills</h4>

          <router-link to="/profile/skills" class="btn btn-outline-primary btn-sm">
            Skills bearbeiten
          </router-link>
        </div>

        <div v-if="hasUserSkills">
          <span v-for="skill in dashboard.userSkills" :key="skill" class="badge bg-success me-2 mb-2">
            {{ skill }}
          </span>
        </div>

        <BaseEmptyState v-else message="Du hast noch keine Skills hinterlegt." />
      </BaseCard>

      <BaseCard v-if="hasMissingSkills" title="Empfohlene Skills" class="mt-4">
        <div class="mt-3">
          <span v-for="skill in dashboard.missingSkills" :key="skill" class="badge bg-danger me-2 mb-2">
            ✗ {{ skill }}
          </span>
        </div>
      </BaseCard>

      <BaseCard v-if="hasRecommendedCourses" title="Empfohlene Kurse" class="mt-4">
        <div class="mt-3">
          <router-link v-for="course in dashboard.recommendedCourses" :key="course.id" :to="`/courses/${course.id}`"
            class="badge bg-primary me-2 mb-2 text-decoration-none">
            {{ course.title }}
          </router-link>
        </div>
      </BaseCard>

      <BaseCard v-if="hasTopJobMatches" title="Top Job Matches" class="mt-4">
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
      </BaseCard>
    </template>
  </div>
</template>