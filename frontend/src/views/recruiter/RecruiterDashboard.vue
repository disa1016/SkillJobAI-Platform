<script setup>
import { computed, onMounted, ref } from "vue";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

import { getRecruiterDashboard } from "@/services/recruiterService";
import { getStatusBadgeClass } from "@/utils/badge";
import { formatDate } from "@/utils/date";

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

const statCards = computed(() => [
  { label: "Unternehmen", value: dashboard.value?.totalCompanies ?? 0, icon: "bi-buildings" },
  { label: "Stellenangebote", value: dashboard.value?.totalJobs ?? 0, icon: "bi-briefcase" },
  { label: "Bewerbungen", value: dashboard.value?.totalApplications ?? 0, icon: "bi-file-earmark-person" },
  { label: "Ausstehend", value: dashboard.value?.pendingApplications ?? 0, icon: "bi-hourglass-split" },
  { label: "Geprüft", value: dashboard.value?.reviewedApplications ?? 0, icon: "bi-eye" },
  { label: "Angenommen", value: dashboard.value?.acceptedApplications ?? 0, icon: "bi-check-circle" },
  { label: "Abgelehnt", value: dashboard.value?.rejectedApplications ?? 0, icon: "bi-x-circle" },
]);

const recentApplications = computed(() => dashboard.value?.recentApplications ?? []);
const topJobsByApplications = computed(() => dashboard.value?.topJobsByApplications ?? []);

const loadDashboard = async () => {
  loading.value = true;
  error.value = "";

  try {
    dashboard.value = await getRecruiterDashboard();
  } catch {
    error.value = "Dashboard konnte nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};

onMounted(loadDashboard);
</script>

<template>
  <main class="container py-4">
    <PageHeader title="Recruiter-Dashboard"
      description="Überblick über Stellenangebote, Bewerbungen und aktuelle Aktivitäten.">
      <template #actions>
        <router-link to="/recruiter/jobs" class="btn btn-outline-primary">
          <i class="bi bi-briefcase me-2" aria-hidden="true"></i>
          Jobs verwalten
        </router-link>

        <router-link to="/recruiter/applications" class="btn btn-primary">
          <i class="bi bi-file-earmark-person me-2" aria-hidden="true"></i>
          Bewerbungen verwalten
        </router-link>
      </template>
    </PageHeader>

    <BaseSpinner v-if="loading" message="Dashboard wird geladen..." />

    <BaseAlert v-else-if="error" type="danger" :message="error" />

    <template v-else-if="dashboard">
      <div class="row g-3 mb-4">
        <div v-for="stat in statCards" :key="stat.label" class="col-12 col-sm-6 col-lg-4 col-xl-3">
          <BaseCard>
            <div class="d-flex justify-content-between align-items-start gap-3">
              <div>
                <p class="text-body-secondary mb-2">{{ stat.label }}</p>
                <p class="h2 mb-0">{{ stat.value }}</p>
              </div>

              <i class="bi fs-3 text-primary" :class="stat.icon" aria-hidden="true"></i>
            </div>
          </BaseCard>
        </div>
      </div>

      <div class="row g-4">
        <div class="col-12 col-xl-7">
          <BaseCard title="Neueste Bewerbungen" body-class="p-0">
            <div v-if="recentApplications.length" class="list-group list-group-flush">
              <div v-for="application in recentApplications" :key="application.id" class="list-group-item p-3 p-md-4">
                <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-start gap-3">
                  <div>
                    <h3 class="h6 mb-1">
                      {{ application.candidate?.fullName || "Unbekannter Kandidat" }}
                    </h3>

                    <p class="text-body-secondary small mb-2">
                      {{ application.candidate?.email || "Keine E-Mail" }}
                    </p>

                    <p class="mb-1">
                      <span class="fw-semibold">Job:</span>
                      {{ application.job?.title || "Job gelöscht" }}
                    </p>

                    <p class="text-body-secondary small mb-0">
                      {{ application.job?.company || "Keine Firma" }}
                      · {{ formatDate(application.createdAt) }}
                    </p>
                  </div>

                  <span class="badge" :class="getStatusBadgeClass(application.status)">
                    {{ application.status || "Unknown" }}
                  </span>
                </div>
              </div>
            </div>

            <BaseEmptyState v-else title="Keine Bewerbungen" message="Noch sind keine Bewerbungen vorhanden."
              icon="bi-file-earmark-x" />

            <template #footer>
              <router-link to="/recruiter/applications" class="btn btn-outline-primary btn-sm">
                Alle Bewerbungen ansehen
              </router-link>
            </template>
          </BaseCard>
        </div>

        <div class="col-12 col-xl-5">
          <BaseCard title="Top Jobs nach Bewerbungen" body-class="p-0">
            <div v-if="topJobsByApplications.length" class="list-group list-group-flush">
              <div v-for="item in topJobsByApplications" :key="item.jobId"
                class="list-group-item d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-2 p-3">
                <div>
                  <h3 class="h6 mb-1">{{ item.job?.title || "Job gelöscht" }}</h3>
                  <p class="text-body-secondary small mb-0">
                    {{ item.job?.company || "Keine Firma" }}
                  </p>
                </div>

                <span class="badge text-bg-primary">
                  {{ item.applicationsCount }} Bewerbungen
                </span>
              </div>
            </div>

            <BaseEmptyState v-else title="Keine Job-Statistiken"
              message="Noch sind keine Bewerbungen für Stellenangebote vorhanden." icon="bi-bar-chart" />
          </BaseCard>
        </div>
      </div>
    </template>

    <BaseEmptyState v-else title="Keine Dashboard-Daten" message="Es konnten keine Dashboard-Daten gefunden werden." />
  </main>
</template>
