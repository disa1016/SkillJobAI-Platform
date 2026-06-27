<script setup>
import { computed, onMounted, ref } from "vue";
import api from "../../services/api";

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
const topJobsByApplications = computed(() => dashboard.value?.topJobsByApplications ?? []);

const getStatusBadgeClass = (status) => {
  const statusClasses = {
    Accepted: "bg-success",
    Rejected: "bg-danger",
    Reviewed: "bg-info text-dark",
    Pending: "bg-warning text-dark",
  };

  return statusClasses[status] || "bg-secondary";
};

const formatDate = (date) => {
  if (!date) return "Kein Datum";

  return new Date(date).toLocaleDateString("de-DE", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
};

const loadDashboard = async () => {
  loading.value = true;
  error.value = "";

  try {
    const response = await api.get("/recruiter/dashboard");
    dashboard.value = response.data;
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

    <div v-if="loading" class="alert alert-info">
      Dashboard wird geladen...
    </div>

    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <template v-else-if="dashboard">
      <div class="row g-3 mb-4">
        <div v-for="stat in statCards" :key="stat.label" class="col-md-3">
          <div class="card shadow-sm border-0 h-100">
            <div class="card-body">
              <h6 class="text-muted">{{ stat.label }}</h6>
              <h3 class="mb-0">{{ stat.value }}</h3>
            </div>
          </div>
        </div>
      </div>

      <div class="row g-3">
        <div class="col-lg-7">
          <div class="card shadow-sm h-100">
            <div class="card-body">
              <h5 class="mb-3">Neueste Bewerbungen</h5>

              <div v-if="recentApplications.length === 0" class="text-muted">
                Noch keine Bewerbungen vorhanden.
              </div>

              <div v-for="application in recentApplications" :key="application.id" class="border rounded p-3 mb-3">
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
            </div>
          </div>
        </div>

        <div class="col-lg-5">
          <div class="card shadow-sm h-100">
            <div class="card-body">
              <h5 class="mb-3">Top Jobs nach Bewerbungen</h5>

              <div v-if="topJobsByApplications.length === 0" class="text-muted">
                Noch keine Bewerbungen vorhanden.
              </div>

              <div v-for="item in topJobsByApplications" :key="item.jobId"
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
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>