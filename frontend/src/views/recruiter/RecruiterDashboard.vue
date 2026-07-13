<script setup>
import { computed, onMounted, ref } from "vue";
import { getRecruiterDashboard } from "@/services/recruiterService";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";


import { getStatusBadgeClass } from "@/utils/badge";
import { formatDate } from "@/utils/date";

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

const statCards = computed(() => [
  {
    label: "Companies",
    value: dashboard.value?.totalCompanies ?? 0,
  },
  {
    label: "Jobs",
    value: dashboard.value?.totalJobs ?? 0,
  },
  {
    label: "Applications",
    value: dashboard.value?.totalApplications ?? 0,
  },
  {
    label: "Pending",
    value: dashboard.value?.pendingApplications ?? 0,
  },
  {
    label: "Reviewed",
    value: dashboard.value?.reviewedApplications ?? 0,
  },
  {
    label: "Accepted",
    value: dashboard.value?.acceptedApplications ?? 0,
  },
  {
    label: "Rejected",
    value: dashboard.value?.rejectedApplications ?? 0,
  },
]);

const recentApplications = computed(() => dashboard.value?.recentApplications ?? []);

const topJobsByApplications = computed(() => {
  return dashboard.value?.topJobsByApplications ?? [];
});

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
  <div class="container py-4">
    <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
      <h2 class="mb-0">Recruiter Dashboard</h2>

      <div class="d-flex flex-wrap gap-2">
        <router-link to="/recruiter/jobs" class="btn btn-outline-primary">
          Jobs verwalten
        </router-link>

        <router-link to="/recruiter/applications" class="btn btn-primary">
          Bewerbungen verwalten
        </router-link>
      </div>
    </div>

    <BaseSpinner v-if="loading" message="Dashboard wird geladen..." />

    <BaseAlert v-else-if="error" type="danger" :message="error" />

    <template v-else-if="dashboard">
      <div class="row g-3 mb-4">
        <div v-for="stat in statCards" :key="stat.label" class="col-md-3">
          <BaseCard>
            <h6 class="text-muted">
              {{ stat.label }}
            </h6>

            <h3 class="mb-0">
              {{ stat.value }}
            </h3>
          </BaseCard>
        </div>
      </div>

      <div class="row g-3">
        <div class="col-lg-7">
          <BaseCard title="Neueste Bewerbungen">
            <BaseEmptyState v-if="recentApplications.length === 0" message="Noch keine Bewerbungen vorhanden." />

            <div v-for="application in recentApplications" v-else :key="application.id" class="border rounded p-3 mb-3">
              <div class="d-flex justify-content-between align-items-start gap-3">
                <div>
                  <h6 class="mb-1">
                    {{ application.candidate?.fullName || "Unbekannter Kandidat" }}
                  </h6>

                  <p class="text-muted mb-1">
                    {{ application.candidate?.email || "Keine E-Mail" }}
                  </p>

                  <p class="mb-1">
                    <strong>Job:</strong>
                    {{ application.job?.title || "Job gelöscht" }}
                  </p>

                  <p class="text-muted mb-0">
                    {{ application.job?.company || "Keine Firma" }}
                    · {{ formatDate(application.createdAt) }}
                  </p>
                </div>

                <span class="badge" :class="getStatusBadgeClass(application.status)">
                  {{ application.status || "Unknown" }}
                </span>
              </div>
            </div>

            <router-link to="/recruiter/applications" class="btn btn-outline-primary btn-sm">
              Alle Bewerbungen ansehen
            </router-link>
          </BaseCard>
        </div>

        <div class="col-lg-5">
          <BaseCard title="Top Jobs nach Bewerbungen">
            <BaseEmptyState v-if="topJobsByApplications.length === 0" message="Noch keine Bewerbungen vorhanden." />

            <div v-for="item in topJobsByApplications" v-else :key="item.jobId"
              class="d-flex justify-content-between align-items-center gap-3 border-bottom py-2">
              <div>
                <strong>
                  {{ item.job?.title || "Job gelöscht" }}
                </strong>

                <div class="text-muted small">
                  {{ item.job?.company || "Keine Firma" }}
                </div>
              </div>

              <span class="badge bg-primary">
                {{ item.applicationsCount }} Bewerbungen
              </span>
            </div>
          </BaseCard>
        </div>
      </div>
    </template>
  </div>
</template>