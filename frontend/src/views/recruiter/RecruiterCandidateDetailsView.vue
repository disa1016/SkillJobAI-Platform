<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import api from "../../services/api";

const route = useRoute();

const candidate = ref(null);
const loading = ref(true);
const error = ref("");

const backendUrl = computed(() => {
    const baseUrl = api.defaults.baseURL || "";
    return baseUrl.replace("/api", "");
});

const hasSkills = computed(() => candidate.value?.skills?.length > 0);
const hasApplications = computed(() => candidate.value?.applications?.length > 0);

const stats = computed(() => [
    {
        label: "Bewerbungen",
        value: candidate.value?.applicationsCount ?? 0,
        textClass: "",
    },
    {
        label: "Accepted",
        value: candidate.value?.acceptedApplications ?? 0,
        textClass: "text-success",
    },
    {
        label: "Rejected",
        value: candidate.value?.rejectedApplications ?? 0,
        textClass: "text-danger",
    },
    {
        label: "Skills",
        value: candidate.value?.skillsCount ?? 0,
        textClass: "",
    },
]);

const getFileUrl = (fileUrl) => {
    if (!fileUrl) return "";
    if (fileUrl.startsWith("http")) return fileUrl;

    return `${backendUrl.value}${fileUrl}`;
};

const hasApplicationFiles = (application) => {
    return Boolean(
        application.cvFileUrl ||
        application.certificateFileUrl ||
        application.portfolioFileUrl
    );
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

const formatDate = (date) => {
    if (!date) return "Kein Datum";

    return new Date(date).toLocaleDateString("de-DE");
};

const loadCandidate = async () => {
    loading.value = true;
    error.value = "";

    try {
        const { data } = await api.get(`/recruiter/candidates/${route.params.id}`);
        candidate.value = data;
    } catch {
        error.value = "Kandidatendetails konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadCandidate);
</script>

<template>
    <div class="container py-4">
        <router-link to="/recruiter/candidates" class="btn btn-outline-secondary mb-3">
            Zurück zu Kandidaten
        </router-link>

        <div v-if="loading" class="alert alert-info">
            Kandidat wird geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else-if="candidate">
            <div class="card shadow-sm mb-4">
                <div class="card-body">
                    <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
                        <div>
                            <h2 class="mb-1">
                                {{ candidate.fullName || "Unbekannter Kandidat" }}
                            </h2>

                            <p class="text-muted mb-1">
                                {{ candidate.email || "Keine E-Mail" }}
                            </p>

                            <p class="text-muted mb-0">
                                Registriert am {{ formatDate(candidate.createdAt) }}
                            </p>
                        </div>

                        <span class="badge bg-primary fs-6">
                            {{ candidate.skillsCount || 0 }} Skills
                        </span>
                    </div>

                    <div class="mt-3">
                        <a v-if="candidate.cvUrl" :href="getFileUrl(candidate.cvUrl)" target="_blank"
                            rel="noopener noreferrer" class="btn btn-outline-primary btn-sm">
                            Profil-CV öffnen
                        </a>

                        <span v-else class="text-muted">
                            Kein Profil-CV hochgeladen.
                        </span>
                    </div>
                </div>
            </div>

            <div class="row g-3 mb-4">
                <div v-for="stat in stats" :key="stat.label" class="col-md-3">
                    <div class="card shadow-sm border-0 h-100">
                        <div class="card-body">
                            <h6 class="text-muted">
                                {{ stat.label }}
                            </h6>

                            <h3 class="mb-0" :class="stat.textClass">
                                {{ stat.value }}
                            </h3>
                        </div>
                    </div>
                </div>
            </div>

            <div class="card shadow-sm mb-4">
                <div class="card-body">
                    <h5 class="mb-3">Skills</h5>

                    <div v-if="hasSkills">
                        <span v-for="skill in candidate.skills" :key="skill" class="badge bg-success me-2 mb-2">
                            {{ skill }}
                        </span>
                    </div>

                    <p v-else class="text-muted mb-0">
                        Keine Skills hinterlegt.
                    </p>
                </div>
            </div>

            <div class="card shadow-sm">
                <div class="card-body">
                    <h5 class="mb-3">Bewerbungen</h5>

                    <div v-if="!hasApplications" class="text-muted">
                        Keine Bewerbungen vorhanden.
                    </div>

                    <div v-for="application in candidate.applications" v-else :key="application.id"
                        class="border rounded p-3 mb-3">
                        <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
                            <div>
                                <h6 class="mb-1">
                                    {{ application.job?.title || "Job gelöscht" }}
                                </h6>

                                <p class="text-muted mb-1">
                                    {{ application.job?.company || "Keine Firma" }}
                                    · {{ formatDate(application.createdAt) }}
                                </p>

                                <p class="mb-1">
                                    <strong>Standort:</strong>
                                    {{ application.job?.location || "Keine Angabe" }}
                                </p>

                                <p class="mb-1">
                                    <strong>Gehalt:</strong>
                                    {{ application.job?.salary || "Keine Angabe" }}
                                </p>
                            </div>

                            <span class="badge" :class="getStatusBadgeClass(application.status)">
                                {{ application.status || "Unbekannt" }}
                            </span>
                        </div>

                        <div class="mt-3">
                            <strong>Bewerbungsunterlagen:</strong>

                            <div class="d-flex flex-wrap gap-2 mt-2">
                                <a v-if="application.cvFileUrl" :href="getFileUrl(application.cvFileUrl)"
                                    target="_blank" rel="noopener noreferrer" class="btn btn-sm btn-outline-primary">
                                    CV öffnen
                                </a>

                                <a v-if="application.certificateFileUrl"
                                    :href="getFileUrl(application.certificateFileUrl)" target="_blank"
                                    rel="noopener noreferrer" class="btn btn-sm btn-outline-secondary">
                                    Zeugnis öffnen
                                </a>

                                <a v-if="application.portfolioFileUrl" :href="getFileUrl(application.portfolioFileUrl)"
                                    target="_blank" rel="noopener noreferrer" class="btn btn-sm btn-outline-dark">
                                    Portfolio öffnen
                                </a>

                                <span v-if="!hasApplicationFiles(application)" class="text-muted">
                                    Keine Dateien hochgeladen.
                                </span>
                            </div>
                        </div>

                        <div v-if="application.coverLetter" class="mt-3">
                            <strong>Anschreiben:</strong>

                            <div class="border rounded bg-light p-3 mt-2">
                                {{ application.coverLetter }}
                            </div>
                        </div>

                        <router-link :to="`/recruiter/applications/${application.id}`"
                            class="btn btn-sm btn-outline-primary mt-3">
                            Bewerbung Details ansehen
                        </router-link>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>