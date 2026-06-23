<script setup>
import { onMounted, ref, computed } from "vue";
import { useRoute } from "vue-router";
import api from "../services/api";

const route = useRoute();

const application = ref(null);
const loading = ref(true);
const error = ref("");
const success = ref("");

const backendUrl = computed(() => {
    const baseUrl = api.defaults.baseURL || "";
    return baseUrl.replace("/api", "");
});

const getFileUrl = (fileUrl) => {
    if (!fileUrl) return "";
    if (fileUrl.startsWith("http")) return fileUrl;
    return `${backendUrl.value}${fileUrl}`;
};

const getStatusBadgeClass = (status) => {
    if (status === "Accepted") return "bg-success";
    if (status === "Rejected") return "bg-danger";
    if (status === "Reviewed") return "bg-info text-dark";
    return "bg-warning text-dark";
};

const loadApplication = async () => {
    try {
        const response = await api.get(`/applications/${route.params.id}`);
        application.value = response.data;
    } catch {
        error.value = "Bewerbungsdetails konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const updateStatus = async (status) => {
    error.value = "";
    success.value = "";

    try {
        await api.put(`/applications/${route.params.id}/status`, {
            status,
        });

        success.value = `Status wurde auf ${status} gesetzt.`;
        await loadApplication();
    } catch {
        error.value = "Status konnte nicht geändert werden.";
    }
};

onMounted(loadApplication);
</script>

<template>
    <div class="container py-4">
        <router-link to="/recruiter/applications" class="btn btn-outline-secondary mb-3">
            Zurück
        </router-link>

        <div v-if="loading" class="alert alert-info">
            Bewerbung wird geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div v-if="application" class="card shadow-sm">
            <div class="card-body">
                <h2>{{ application.candidate?.fullName }}</h2>

                <p class="text-muted">
                    {{ application.candidate?.email }}
                </p>

                <p>
                    <strong>Job:</strong> {{ application.job?.title }}
                </p>

                <p>
                    <strong>Firma:</strong> {{ application.job?.company || "Keine Firma" }}
                </p>

                <p>
                    <strong>Status:</strong>
                    <span class="badge" :class="getStatusBadgeClass(application.status)">
                        {{ application.status }}
                    </span>
                </p>

                <p>
                    <strong>Match Score:</strong> {{ application.matchPercentage }}%
                </p>

                <hr />

                <h5>Bewerbungsunterlagen</h5>

                <div class="d-flex flex-wrap gap-2 mb-4">
                    <a v-if="application.cvFileUrl" :href="getFileUrl(application.cvFileUrl)" target="_blank"
                        class="btn btn-outline-primary">
                        CV öffnen
                    </a>

                    <a v-if="application.certificateFileUrl" :href="getFileUrl(application.certificateFileUrl)"
                        target="_blank" class="btn btn-outline-secondary">
                        Zeugnis öffnen
                    </a>

                    <a v-if="application.portfolioFileUrl" :href="getFileUrl(application.portfolioFileUrl)"
                        target="_blank" class="btn btn-outline-dark">
                        Portfolio öffnen
                    </a>

                    <span v-if="
                        !application.cvFileUrl &&
                        !application.certificateFileUrl &&
                        !application.portfolioFileUrl
                    " class="text-muted">
                        Keine Dateien hochgeladen.
                    </span>
                </div>

                <h5>Anschreiben</h5>

                <div class="border rounded p-3 bg-light mb-4">
                    {{ application.coverLetter || "Kein Anschreiben vorhanden." }}
                </div>

                <h5>Skills</h5>

                <div class="mb-3">
                    <strong>Passende Skills:</strong>
                    <div class="mt-2">
                        <span v-for="skill in application.matchedSkills" :key="skill"
                            class="badge bg-success me-2 mb-2">
                            {{ skill }}
                        </span>
                    </div>
                </div>

                <div class="mb-4">
                    <strong>Fehlende Skills:</strong>
                    <div class="mt-2">
                        <span v-for="skill in application.missingSkills" :key="skill" class="badge bg-danger me-2 mb-2">
                            {{ skill }}
                        </span>
                    </div>
                </div>

                <div class="d-flex gap-2">
                    <button class="btn btn-outline-info" @click="updateStatus('Reviewed')">
                        Reviewed
                    </button>

                    <button class="btn btn-outline-success" @click="updateStatus('Accepted')">
                        Accept
                    </button>

                    <button class="btn btn-outline-danger" @click="updateStatus('Rejected')">
                        Reject
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>