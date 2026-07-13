<script setup>
import { computed, onMounted, ref } from "vue";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";

import { getCandidateDashboard } from "@/services/candidateService";
import { getMatchBadgeClass } from "@/utils/badge";
import { getCurrentUser } from "@/utils/storage";

const user = getCurrentUser();

const dashboard = ref(null);
const loading = ref(false);
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
  <div class="container py-4">
    <div class="mb-4">
      <h1 class="mb-2">
        Willkommen {{ user?.fullName || "" }}
      </h1>

      <p class="text-muted mb-0">
        Rolle: {{ user?.role || "Unbekannt" }}
      </p>
    </div>

    <BaseSpinner v-if="loading" text="Dashboard wird geladen..." />

    <BaseAlert v-else-if="error" type="danger">
      {{ error }}
    </BaseAlert>

    <template v-else-if="dashboard">
      <div class="row g-3">
        <div v-for="stat in stats" :key="stat.title" class="col-md-4">
          <BaseCard :card-class="stat.cardClass">
            <h5>{{ stat.title }}</h5>

            <p class="display-5 mb-0" :class="stat.textClass">
              {{ stat.value }}
            </p>
          </BaseCard>
        </div>
      </div>

      <BaseCard class="mt-4">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
          <h4 class="mb-0">Meine Skills</h4>

          <router-link to="/profile/skills" class="btn btn-outline-primary btn-sm">
            Skills bearbeiten
          </router-link>
        </div>

        <div v-if="hasUserSkills">
          <span
            v-for="skill in dashboard.userSkills"
            :key="skill"
            class="badge bg-success me-2 mb-2"
          >
            {{ skill }}
          </span>
        </div>

        <BaseEmptyState
          v-else
          title="Keine Skills hinterlegt"
          text="Du hast noch keine Skills hinterlegt."
        />
      </BaseCard>

      <BaseCard v-if="hasMissingSkills" class="mt-4">
        <h4 class="mb-3">Empfohlene Skills</h4>

        <span
          v-for="skill in dashboard.missingSkills"
          :key="skill"
          class="badge bg-danger me-2 mb-2"
        >
          ✗ {{ skill }}
        </span>
      </BaseCard>

      <BaseCard v-if="hasRecommendedCourses" class="mt-4">
        <h4 class="mb-3">Empfohlene Kurse</h4>

        <router-link
          v-for="course in dashboard.recommendedCourses"
          :key="course.id"
          :to="`/courses/${course.id}`"
          class="badge bg-primary me-2 mb-2 text-decoration-none"
        >
          {{ course.title }}
        </router-link>
      </BaseCard>

      <BaseCard v-if="hasTopJobMatches" class="mt-4">
        <h4 class="mb-3">Top Job Matches</h4>

        <div
          v-for="job in dashboard.topJobMatches"
          :key="job.id"
          class="border rounded p-3 mb-3"
        >
          <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
            <div>
              <h5 class="mb-1">
                {{ job.title }}
              </h5>

              <p class="text-muted mb-0">
                {{ job.company?.name || "Keine Firma" }}
              </p>
            </div>

            <span
              class="badge"
              :class="getMatchBadgeClass(job.matchPercentage)"
            >
              {{ job.matchPercentage }}%
            </span>
          </div>

          <router-link
            :to="`/jobs/${job.id}`"
            class="btn btn-outline-primary btn-sm"
          >
            Job ansehen
          </router-link>
        </div>
      </BaseCard>
    </template>

    <BaseEmptyState
      v-else
      title="Keine Dashboard-Daten"
      text="Es konnten keine Dashboard-Daten gefunden werden."
    />
  </div>
</template>