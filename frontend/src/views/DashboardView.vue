<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const user = JSON.parse(localStorage.getItem("user"));

const applications = ref([]);
const enrollments = ref([]);
const progress = ref([]);

const loading = ref(true);
const error = ref("");

onMounted(async () => {
  try {
    const applicationsResponse = await api.get("/applications/my");
    const enrollmentsResponse = await api.get("/enrollments/my");
    const progressResponse = await api.get("/progress/my");

    applications.value = applicationsResponse.data;
    enrollments.value = enrollmentsResponse.data;
    progress.value = progressResponse.data;
  } catch {
    error.value = "Dashboard-Daten konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <div class="container mt-4">
    <h1 class="mb-2">
      Willkommen {{ user?.fullName }}
    </h1>

    <p class="text-muted mb-4">
      Rolle: {{ user?.role }}
    </p>

    <div v-if="loading" class="alert alert-info">
      Dashboard wird geladen...
    </div>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div class="row">
      <div class="col-md-4 mb-3">
        <div class="card shadow-sm border-primary h-100">
          <div class="card-body">
            <h5>Meine Bewerbungen</h5>
            <p class="display-5 text-primary">
              {{ applications.length }}
            </p>
          </div>
        </div>
      </div>

      <div class="col-md-4 mb-3">
        <div class="card shadow-sm border-success h-100">
          <div class="card-body">
            <h5>Meine Kurse</h5>
            <p class="display-5 text-success">
              {{ enrollments.length }}
            </p>
          </div>
        </div>
      </div>

      <div class="col-md-4 mb-3">
        <div class="card shadow-sm border-warning h-100">
          <div class="card-body">
            <h5>Abgeschlossene Lektionen</h5>
            <p class="display-5 text-warning">
              {{ progress.length }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>