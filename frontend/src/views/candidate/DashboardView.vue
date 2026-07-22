<script setup>
import { computed, onMounted, ref } from "vue";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

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
    icon: "bi-file-earmark-text",
  },
  {
    title: "Meine Kurse",
    value: dashboard.value?.enrollmentsCount ?? 0,
    icon: "bi-journal-bookmark",
  },
  {
    title: "Abgeschlossene Lektionen",
    value: dashboard.value?.completedLessonsCount ?? 0,
    icon: "bi-check2-circle",
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
  <main class="container py-4">
    <PageHeader :title="`Willkommen${user?.fullName ? `, ${user.fullName}` : ''}`"
      description="Hier findest du einen Überblick über deine Bewerbungen, Skills und Empfehlungen.">
      <template #actions>
        <router-link to="/profile/skills" class="btn btn-outline-primary">
          <i class="bi bi-pencil-square me-2" aria-hidden="true"></i>
          Skills bearbeiten
        </router-link>
      </template>
    </PageHeader>

    <BaseSpinner v-if="loading" message="Dashboard wird geladen..." />

    <BaseAlert v-else-if="error" type="danger" :message="error" />

    <template v-else-if="dashboard">
      <div class="row g-3 mb-4">
        <div v-for="stat in stats" :key="stat.title" class="col-12 col-md-4">
          <BaseCard>
            <div class="d-flex justify-content-between align-items-start gap-3">
              <div>
                <p class="text-body-secondary mb-2">{{ stat.title }}</p>
                <p class="h2 mb-0">{{ stat.value }}</p>
              </div>

              <i class="bi fs-3 text-primary" :class="stat.icon" aria-hidden="true"></i>
            </div>
          </BaseCard>
        </div>
      </div>

      <div class="row g-4">
        <div class="col-12 col-xl-7">
          <BaseCard title="Meine Skills">
            <div v-if="hasUserSkills" class="d-flex flex-wrap gap-2">
              <span v-for="skill in dashboard.userSkills" :key="skill" class="badge text-bg-success">
                {{ skill }}
              </span>
            </div>

            <BaseEmptyState v-else title="Keine Skills hinterlegt" message="Du hast noch keine Skills hinterlegt."
              icon="bi-lightbulb">
              <template #actions>
                <router-link to="/profile/skills" class="btn btn-primary btn-sm">
                  Skills hinzufügen
                </router-link>
              </template>
            </BaseEmptyState>
          </BaseCard>
        </div>

        <div class="col-12 col-xl-5">
          <BaseCard title="Empfohlene Skills">
            <div v-if="hasMissingSkills" class="d-flex flex-wrap gap-2">
              <span v-for="skill in dashboard.missingSkills" :key="skill" class="badge text-bg-danger">
                {{ skill }}
              </span>
            </div>

            <BaseEmptyState v-else title="Keine offenen Skill-Empfehlungen"
              message="Aktuell wurden keine fehlenden Skills ermittelt." icon="bi-check-circle" />
          </BaseCard>
        </div>

        <div class="col-12">
          <BaseCard title="Empfohlene Kurse">
            <div v-if="hasRecommendedCourses" class="d-flex flex-wrap gap-2">
              <router-link v-for="course in dashboard.recommendedCourses" :key="course.id" :to="`/courses/${course.id}`"
                class="btn btn-outline-primary btn-sm">
                <i class="bi bi-book me-2" aria-hidden="true"></i>
                {{ course.title }}
              </router-link>
            </div>

            <BaseEmptyState v-else title="Keine Kursempfehlungen"
              message="Aktuell sind keine passenden Kurse verfügbar." icon="bi-journal-x" />
          </BaseCard>
        </div>

        <div class="col-12">
          <BaseCard title="Top Job Matches" body-class="p-0">
            <div v-if="hasTopJobMatches" class="list-group list-group-flush">
              <div v-for="job in dashboard.topJobMatches" :key="job.id" class="list-group-item p-3 p-md-4">
                <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3">
                  <div>
                    <h3 class="h5 mb-1">{{ job.title }}</h3>
                    <p class="text-body-secondary mb-0">
                      {{ job.company?.name || "Keine Firma" }}
                    </p>
                  </div>

                  <div class="d-flex flex-wrap align-items-center gap-2">
                    <span class="badge" :class="getMatchBadgeClass(job.matchPercentage)">
                      {{ job.matchPercentage }} % Match
                    </span>

                    <router-link :to="`/jobs/${job.id}`" class="btn btn-outline-primary btn-sm">
                      Job ansehen
                    </router-link>
                  </div>
                </div>
              </div>
            </div>

            <BaseEmptyState v-else title="Keine Job-Matches"
              message="Aktuell konnten keine passenden Stellenangebote gefunden werden." icon="bi-briefcase" />
          </BaseCard>
        </div>
      </div>
    </template>

    <BaseEmptyState v-else title="Keine Dashboard-Daten" message="Es konnten keine Dashboard-Daten gefunden werden." />
  </main>
</template>
