<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import api from "../../services/api";
const route = useRoute();

const application = ref(null);

const loading = ref(true);
const updating = ref(false);

const error = ref("");
const success = ref("");

const backendUrl = computed(() => {
    const baseUrl = api.defaults.baseURL || "";
    return baseUrl.replace("/api", "");
});

const hasApplicationFiles = computed(() => {
    return Boolean(
        application.value?.cvFileUrl ||
        application.value?.certificateFileUrl ||
        application.value?.portfolioFileUrl
    );
});

const hasMatchedSkills = computed(() => {
    return application.value?.matchedSkills?.length > 0;
});

const hasMissingSkills = computed(() => {
    return application.value?.missingSkills?.length > 0;
});

const getFileUrl = (fileUrl) => {
    if (!fileUrl) return "";
    if (fileUrl.startsWith("http")) return fileUrl;

    return `${backendUrl.value}${fileUrl}`;
};

const getStatusBadgeClass = (status) => {
    const statusClasses = {
        Accepted: "bg-success",
        Rejected: "bg-danger",
        Reviewed: "bg-info text-dark",
        Pending: "bg-warning text-dark",
    };

    return statusClasses[status] || "bg-secondary";
};

const getScoreBadgeClass = (score) => {
    const percentage = Number(score) || 0;

    if (percentage >= 70) return "bg-success";
    if (percentage >= 40) return "bg-warning text-dark";

    return "bg-danger";
};

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const loadApplication = async () => {
    loading.value = true;
    error.value = "";

    try {
        const { data } = await api.get(`/applications/${route.params.id}`);
        application.value = data;
    } catch {
        error.value = "Bewerbungsdetails konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const updateStatus = async (status) => {
    clearMessages();
    updating.value = true;

    try {
        await api.put(`/applications/${route.params.id}/status`, {
            status,
        });

        success.value = `Status wurde auf ${status} gesetzt.`;
        await loadApplication();
    } catch {
        error.value = "Status konnte nicht geändert werden.";
    } finally {
        updating.value = false;
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

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else-if="application">
            <div v-if="success" class="alert alert-success">
                {{ success }}
            </div>

            <div class="card shadow-sm">
                <div class="card-body">
                    <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-3">
                        <div>
                            <h2 class="mb-1">
                                {{ application.candidate?.fullName || "Unbekannter Kandidat" }}
                            </h2>

                            <p class="text-muted mb-0">
                                {{ application.candidate?.email || "Keine E-Mail" }}
                            </p>
                        </div>

                        <span class="badge fs-6" :class="getStatusBadgeClass(application.status)">
                            {{ application.status || "Unbekannt" }}
                        </span>
                    </div>

                    <p>
                        <strong>Job:</strong>
                        {{ application.job?.title || "Job gelöscht" }}
                    </p>

                    <p>
                        <strong>Firma:</strong>
                        {{ application.job?.company || "Keine Firma" }}
                    </p>

                    <p>
                        <strong>Match Score:</strong>
                        <span class="badge" :class="getScoreBadgeClass(application.matchPercentage)">
                            {{ application.matchPercentage ?? 0 }}%
                        </span>
                    </p>

                    <hr />

                    <h5>Bewerbungsunterlagen</h5>

                    <div class="d-flex flex-wrap gap-2 mb-4">
                        <a v-if="application.cvFileUrl" :href="getFileUrl(application.cvFileUrl)" target="_blank"
                            rel="noopener noreferrer" class="btn btn-outline-primary">
                            CV öffnen
                        </a>

                        <a v-if="application.certificateFileUrl" :href="getFileUrl(application.certificateFileUrl)"
                            target="_blank" rel="noopener noreferrer" class="btn btn-outline-secondary">
                            Zeugnis öffnen
                        </a>

                        <a v-if="application.portfolioFileUrl" :href="getFileUrl(application.portfolioFileUrl)"
                            target="_blank" rel="noopener noreferrer" class="btn btn-outline-dark">
                            Portfolio öffnen
                        </a>

                        <span v-if="!hasApplicationFiles" class="text-muted">
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

                        <div v-if="hasMatchedSkills" class="mt-2">
                            <span v-for="skill in application.matchedSkills" :key="skill"
                                class="badge bg-success me-2 mb-2">
                                {{ skill }}
                            </span>
                        </div>

                        <p v-else class="text-muted mt-2 mb-0">
                            Keine passenden Skills gefunden.
                        </p>
                    </div>

                    <div class="mb-4">
                        <strong>Fehlende Skills:</strong>

                        <div v-if="hasMissingSkills" class="mt-2">
                            <span v-for="skill in application.missingSkills" :key="skill"
                                class="badge bg-danger me-2 mb-2">
                                {{ skill }}
                            </span>
                        </div>

                        <p v-else class="text-muted mt-2 mb-0">
                            Keine fehlenden Skills gefunden.
                        </p>
                    </div>

                    <div class="d-flex flex-wrap gap-2">
                        <button type="button" class="btn btn-outline-info" :disabled="updating"
                            @click="updateStatus('Reviewed')">
                            Reviewed
                        </button>

                        <button type="button" class="btn btn-outline-success" :disabled="updating"
                            @click="updateStatus('Accepted')">
                            Accept
                        </button>

                        <button type="button" class="btn btn-outline-danger" :disabled="updating"
                            @click="updateStatus('Rejected')">
                            Reject
                        </button>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>