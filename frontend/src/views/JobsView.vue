<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const jobs = ref([]);
const loading = ref(true);
const error = ref("");

onMounted(async () => {
    try {
        const response = await api.get("/jobs");
        jobs.value = response.data;
    } catch {
        error.value = "Jobs konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="container mt-4">
        <h1 class="mb-4">Jobs</h1>

        <div v-if="loading" class="alert alert-info">
            Jobs werden geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div class="row">
            <div v-for="job in jobs" :key="job.id" class="col-md-4 mb-3">
                <div class="card shadow-sm h-100">
                    <div class="card-body">

                        <h5 class="card-title">
                            {{ job.title }}
                        </h5>

                        <p v-if="job.company" class="mb-2 text-muted">
                            Firma:
                            <router-link :to="`/companies/${job.company.id}`" class="text-decoration-none fw-semibold">
                                {{ job.company.name }}
                            </router-link>
                        </p>

                        <p class="card-text">
                            {{ job.description }}
                        </p>

                        <span class="badge bg-primary me-2">
                            {{ job.location }}
                        </span>

                        <span class="badge bg-success">
                            {{ job.salary }}
                        </span>
                    </div>

                    <router-link :to="`/jobs/${job.id}`" class="btn btn-primary mt-3 d-block">
                        Details
                    </router-link>
                </div>
            </div>
        </div>
    </div>
</template>