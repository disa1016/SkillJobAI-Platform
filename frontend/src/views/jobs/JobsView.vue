<script setup>
import { computed, onMounted, ref } from "vue";
import api from "../../services/api";

const jobs = ref([]);
const loading = ref(true);
const error = ref("");

const hasJobs = computed(() => jobs.value.length > 0);

const loadJobs = async () => {
    loading.value = true;
    error.value = "";

    try {
        const { data } = await api.get("/jobs");
        jobs.value = data;
    } catch {
        error.value = "Jobs konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadJobs);
</script>

<template>
    <div class="container mt-4">
        <h1 class="mb-4">Jobs</h1>

        <div v-if="loading" class="alert alert-info">
            Jobs werden geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <div v-if="!hasJobs" class="alert alert-light border">
                Aktuell sind keine Jobs verfügbar.
            </div>

            <div v-else class="row">
                <div v-for="job in jobs" :key="job.id" class="col-md-4 mb-3">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h5 class="card-title">
                                {{ job.title || "Ohne Titel" }}
                            </h5>

                            <p v-if="job.company" class="mb-2 text-muted">
                                Firma:
                                <router-link :to="`/companies/${job.company.id}`"
                                    class="text-decoration-none fw-semibold">
                                    {{ job.company.name }}
                                </router-link>
                            </p>

                            <p class="card-text">
                                {{ job.description || "Keine Beschreibung vorhanden." }}
                            </p>

                            <span class="badge bg-primary me-2">
                                {{ job.location || "Kein Standort" }}
                            </span>

                            <span class="badge bg-success">
                                {{ job.salary || "Kein Gehalt angegeben" }}
                            </span>
                        </div>

                        <div class="card-footer bg-white border-0">
                            <router-link :to="`/jobs/${job.id}`" class="btn btn-primary w-100">
                                Details
                            </router-link>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>