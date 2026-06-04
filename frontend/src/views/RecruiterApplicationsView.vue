<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const jobs = ref([]);
const selectedJobId = ref("");
const applications = ref([]);

const loading = ref(false);
const error = ref("");
const success = ref("");

const loadJobs = async () => {
    try {
        const response = await api.get("/jobs");
        jobs.value = response.data;
    } catch {
        error.value = "Jobs konnten nicht geladen werden.";
    }
};

const loadApplications = async () => {
    if (!selectedJobId.value) return;

    loading.value = true;
    error.value = "";
    success.value = "";

    try {
        const response = await api.get(`/applications/job/${selectedJobId.value}`);
        applications.value = response.data;
    } catch {
        error.value = "Bewerbungen konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const updateStatus = async (applicationId, status) => {
    error.value = "";
    success.value = "";

    try {
        await api.put(`/applications/${applicationId}/status`, {
            status,
        });

        success.value = `Status wurde auf ${status} gesetzt.`;
        await loadApplications();
    } catch {
        error.value = "Status konnte nicht geändert werden.";
    }
};

onMounted(loadJobs);
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Recruiter Applications</h2>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div class="card shadow-sm mb-4">
            <div class="card-body">
                <h5>Job auswählen</h5>

                <div class="d-flex gap-2">
                    <select v-model="selectedJobId" class="form-select">
                        <option value="">Job auswählen</option>
                        <option v-for="job in jobs" :key="job.id" :value="job.id">
                            {{ job.title }} - {{ job.company?.name || "Keine Firma" }}
                        </option>
                    </select>

                    <button class="btn btn-primary" @click="loadApplications">
                        Bewerbungen laden
                    </button>
                </div>
            </div>
        </div>

        <div v-if="loading" class="alert alert-info">
            Bewerbungen werden geladen...
        </div>

        <div v-if="applications.length > 0" class="card shadow-sm">
            <div class="card-body">
                <h5>Bewerbungen</h5>

                <div v-for="application in applications" :key="application.id" class="border rounded p-3 mb-3">
                    <h6>{{ application.candidate?.fullName }}</h6>
                    <p class="text-muted mb-1">
                        {{ application.candidate?.email }}
                    </p>

                    <p>
                        <strong>Status:</strong>
                        <span class="badge bg-secondary">
                            {{ application.status }}
                        </span>
                    </p>

                    <p v-if="application.coverLetter">
                        <strong>Anschreiben:</strong><br />
                        {{ application.coverLetter }}
                    </p>

                    <div class="d-flex gap-2">
                        <button class="btn btn-sm btn-outline-info" @click="updateStatus(application.id, 'Reviewed')">
                            Reviewed
                        </button>

                        <button class="btn btn-sm btn-outline-success"
                            @click="updateStatus(application.id, 'Accepted')">
                            Accept
                        </button>

                        <button class="btn btn-sm btn-outline-danger" @click="updateStatus(application.id, 'Rejected')">
                            Reject
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <p v-if="selectedJobId && !loading && applications.length === 0" class="text-muted">
            Für diesen Job gibt es noch keine Bewerbungen.
        </p>
    </div>
</template>