<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

const getStatusBadgeClass = (status) => {
  if (status === "Accepted") return "bg-success";
  if (status === "Rejected") return "bg-danger";
  if (status === "Reviewed") return "bg-info text-dark";
  return "bg-warning text-dark";
};

const formatDate = (date) => {
  return new Date(date).toLocaleDateString("de-DE", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
};

onMounted(async () => {
  try {
    const response = await api.get("/recruiter/dashboard");
    dashboard.value = response.data;
  } catch (err) {
    error.value = "Dashboard konnte nicht geladen werden.";
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <div class="container py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h2 class="mb-0">Recruiter Dashboard</h2>

      <div class="d-flex gap-2">
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

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-if="dashboard">
      <div class="row g-3 mb-4">
        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Companies</h6>
              <h3>{{ dashboard.totalCompanies }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Jobs</h6>
              <h3>{{ dashboard.totalJobs }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Applications</h6>
              <h3>{{ dashboard.totalApplications }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Pending</h6>
              <h3>{{ dashboard.pendingApplications }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Reviewed</h6>
              <h3>{{ dashboard.reviewedApplications }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Accepted</h6>
              <h3>{{ dashboard.acceptedApplications }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Rejected</h6>
              <h3>{{ dashboard.rejectedApplications }}</h3>
            </div>
          </div>
        </div>
      </div>

      <div class="row g-3">
        <div class="col-lg-7">
          <div class="card shadow-sm">
            <div class="card-body">
              <h5 class="mb-3">Neueste Bewerbungen</h5>

              <div v-if="dashboard.recentApplications?.length === 0" class="text-muted">
                Noch keine Bewerbungen vorhanden.
              </div>

              <div v-for="application in dashboard.recentApplications" :key="application.id"
                class="border rounded p-3 mb-3">
                <div class="d-flex justify-content-between align-items-start">
                  <div>
                    <h6 class="mb-1">
                      {{ application.candidate?.fullName || "Unbekannter Kandidat" }}
                    </h6>

                    <p class="text-muted mb-1">
                      {{ application.candidate?.email }}
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
                    {{ application.status }}
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
          <div class="card shadow-sm">
            <div class="card-body">
              <h5 class="mb-3">Top Jobs nach Bewerbungen</h5>

              <div v-if="dashboard.topJobsByApplications?.length === 0" class="text-muted">
                Noch keine Bewerbungen vorhanden.
              </div>

              <div v-for="item in dashboard.topJobsByApplications" :key="item.jobId"
                class="d-flex justify-content-between align-items-center border-bottom py-2">
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
    </div>

  </div>
</template>