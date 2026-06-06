<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const jobs = ref([]);
const loading = ref(true);
const error = ref("");
const success = ref("");

const loadJobs = async () => {
    try {
        const response = await api.get("/jobs");
        jobs.value = response.data;
    } catch {
        error.value = "Jobs konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const deleteJob = async (id) => {
    if (!confirm("Möchtest du diesen Job wirklich löschen?")) return;

    error.value = "";
    success.value = "";

    try {
        await api.delete(`/jobs/${id}`);
        success.value = "Job wurde gelöscht.";
        await loadJobs();
    } catch {
        error.value = "Job konnte nicht gelöscht werden.";
    }
};

onMounted(loadJobs);
</script>

<template>
    <div class="container py-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2>Recruiter Jobs</h2>

            <router-link to="/recruiter/jobs/create" class="btn btn-primary">
                + Neuen Job erstellen
            </router-link>
        </div>

        <div v-if="loading" class="alert alert-info">
            Jobs werden geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div v-if="jobs.length > 0" class="row g-3">
            <div v-for="job in jobs" :key="job.id" class="col-md-6">
                <div class="card shadow-sm h-100">
                    <div class="card-body">
                        <h5>{{ job.title }}</h5>

                        <p class="text-muted mb-1">
                            {{ job.company?.name || "Keine Firma" }} · {{ job.location }}
                        </p>

                        <p>{{ job.description }}</p>

                        <span class="badge bg-success mb-3">
                            {{ job.salary }}
                        </span>

                        <div class="d-flex gap-2 mt-3">
                            <router-link :to="`/jobs/${job.id}`" class="btn btn-sm btn-outline-secondary">
                                Anzeigen
                            </router-link>

                            <router-link :to="`/recruiter/jobs/edit/${job.id}`" class="btn btn-sm btn-outline-primary">
                                Bearbeiten
                            </router-link>

                            <button class="btn btn-sm btn-outline-danger" @click="deleteJob(job.id)">
                                Löschen
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <p v-if="!loading && jobs.length === 0" class="text-muted">
            Noch keine Jobs vorhanden.
        </p>
    </div>
</template>