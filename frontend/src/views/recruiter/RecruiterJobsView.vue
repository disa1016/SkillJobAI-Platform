<script setup>
import { computed, onMounted, ref } from "vue";
import api from "@/services/api";

const jobs = ref([]);
const loading = ref(true);
const error = ref("");
const success = ref("");

const hasJobs = computed(() => jobs.value.length > 0);

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

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

const deleteJob = async (id) => {
    if (!confirm("Möchtest du diesen Job wirklich löschen?")) return;

    clearMessages();

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
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
            <h2 class="mb-0">Recruiter Jobs</h2>

            <router-link to="/recruiter/jobs/create" class="btn btn-primary">
                + Neuen Job erstellen
            </router-link>
        </div>

        <div v-if="loading" class="alert alert-info">
            Jobs werden geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <div v-if="success" class="alert alert-success">
                {{ success }}
            </div>

            <div v-if="!hasJobs" class="alert alert-light border">
                Noch keine Jobs vorhanden.
            </div>

            <div v-else class="row g-3">
                <div v-for="job in jobs" :key="job.id" class="col-md-6">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h5>
                                {{ job.title || "Ohne Titel" }}
                            </h5>

                            <p class="text-muted mb-1">
                                {{ job.company?.name || "Keine Firma" }}
                                · {{ job.location || "Kein Standort" }}
                            </p>

                            <p>
                                {{ job.description || "Keine Beschreibung vorhanden." }}
                            </p>

                            <span class="badge bg-success mb-3">
                                {{ job.salary || "Kein Gehalt angegeben" }}
                            </span>

                            <div class="d-flex flex-wrap gap-2 mt-3">
                                <router-link :to="`/jobs/${job.id}`" class="btn btn-sm btn-outline-secondary">
                                    Anzeigen
                                </router-link>

                                <router-link :to="`/recruiter/jobs/edit/${job.id}`"
                                    class="btn btn-sm btn-outline-primary">
                                    Bearbeiten
                                </router-link>

                                <button type="button" class="btn btn-sm btn-outline-danger" @click="deleteJob(job.id)">
                                    Löschen
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>