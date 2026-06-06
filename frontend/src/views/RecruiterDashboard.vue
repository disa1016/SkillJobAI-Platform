<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

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
    <h2 class="mb-4">Recruiter Dashboard</h2>

    <div v-if="loading" class="alert alert-info">
      Loading dashboard...
    </div>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-if="dashboard" class="row g-3">
      <div class="col-md-3">
        <div class="card shadow-sm">
          <div class="card-body">
            <h6>Companies</h6>
            <h3>{{ dashboard.totalCompanies }}</h3>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm">
          <div class="card-body">
            <h6>Jobs</h6>
            <h3>{{ dashboard.totalJobs }}</h3>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm">
          <div class="card-body">
            <h6>Applications</h6>
            <h3>{{ dashboard.totalApplications }}</h3>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm">
          <div class="card-body">
            <h6>Pending</h6>
            <h3>{{ dashboard.pendingApplications }}</h3>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm">
          <div class="card-body">
            <h6>Reviewed</h6>
            <h3>{{ dashboard.reviewedApplications }}</h3>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm">
          <div class="card-body">
            <h6>Accepted</h6>
            <h3>{{ dashboard.acceptedApplications }}</h3>
          </div>
        </div>
      </div>

      <div class="col-md-3">
        <div class="card shadow-sm">
          <div class="card-body">
            <h6>Rejected</h6>
            <h3>{{ dashboard.rejectedApplications }}</h3>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-4">
      <router-link to="/recruiter/applications" class="btn btn-primary">
        Bewerbungen verwalten
      </router-link>
    </div>

  </div>
</template>