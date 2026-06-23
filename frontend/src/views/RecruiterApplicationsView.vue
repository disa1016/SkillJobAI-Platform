<script setup>
import { onMounted, ref, computed } from "vue";
import api from "../services/api";

const jobs = ref([]);
const selectedJobId = ref("");
const applications = ref([]);

const loading = ref(false);
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

const getScoreBadgeClass = (score) => {
    if (score >= 70) return "bg-success";
    if (score >= 40) return "bg-warning text-dark";
    return "bg-danger";
};

const getStatusBadgeClass = (status) => {
    if (status === "Accepted") return "bg-success";
    if (status === "Rejected") return "bg-danger";
    if (status === "Reviewed") return "bg-info text-dark";
    return "bg-warning text-dark";
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
                <h5 class="mb-3">Bewerbungen Ranking</h5>

                <div v-for="(application, index) in applications" :key="application.id" class="border rounded p-3 mb-3">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <div>
                            <h5 class="mb-1">
                                #{{ index + 1 }} {{ application.candidate?.fullName }}
                            </h5>

                            <p class="text-muted mb-1">
                                {{ application.candidate?.email }}
                            </p>
                        </div>

                        <span class="badge fs-6" :class="getScoreBadgeClass(application.matchPercentage)">
                            Match {{ application.matchPercentage }}%
                        </span>
                    </div>

                    <div class="progress mb-3" style="height: 24px;">
                        <div class="progress-bar" role="progressbar"
                            :class="getScoreBadgeClass(application.matchPercentage)"
                            :style="`width: ${application.matchPercentage}%`">
                            {{ application.matchPercentage }}%
                        </div>
                    </div>

                    <p>
                        <strong>Status:</strong>
                        <span class="badge" :class="getStatusBadgeClass(application.status)">
                            {{ application.status }}
                        </span>
                    </p>

                    <div class="mb-3">
                        <strong>Bewerbungsunterlagen:</strong>

                        <div class="d-flex flex-wrap gap-2 mt-2">
                            <a v-if="application.cvFileUrl" :href="getFileUrl(application.cvFileUrl)" target="_blank"
                                rel="noopener noreferrer" class="btn btn-sm btn-outline-primary">
                                CV öffnen
                            </a>

                            <a v-if="application.certificateFileUrl" :href="getFileUrl(application.certificateFileUrl)"
                                target="_blank" rel="noopener noreferrer" class="btn btn-sm btn-outline-secondary">
                                Zeugnis öffnen
                            </a>

                            <a v-if="application.portfolioFileUrl" :href="getFileUrl(application.portfolioFileUrl)"
                                target="_blank" rel="noopener noreferrer" class="btn btn-sm btn-outline-dark">
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
                    </div>

                    <div v-if="application.matchedSkills?.length" class="mb-2">
                        <strong>Passende Skills:</strong>

                        <div class="mt-2">
                            <span v-for="skill in application.matchedSkills" :key="skill"
                                class="badge bg-success me-2 mb-2">
                                {{ skill }}
                            </span>
                        </div>
                    </div>

                    <div v-if="application.missingSkills?.length" class="mb-2">
                        <strong>Fehlende Skills:</strong>

                        <div class="mt-2">
                            <span v-for="skill in application.missingSkills" :key="skill"
                                class="badge bg-danger me-2 mb-2">
                                ✗ {{ skill }}
                            </span>
                        </div>
                    </div>

                    <div v-if="application.recommendedCourses?.length" class="mb-3">
                        <strong>Empfohlene Kurse:</strong>

                        <div class="mt-2">
                            <router-link v-for="course in application.recommendedCourses"
                                :key="course.id + '-' + course.skill" :to="`/courses/${course.id}`"
                                class="badge bg-primary me-2 mb-2 text-decoration-none">
                                {{ course.title }} für {{ course.skill }}
                            </router-link>
                        </div>
                    </div>

                    <div v-if="
                        (!application.matchedSkills || application.matchedSkills.length === 0) &&
                        (!application.missingSkills || application.missingSkills.length === 0)
                    " class="alert alert-warning">
                        Für diesen Job wurden noch keine Skills hinterlegt.
                    </div>

                    <p v-if="application.coverLetter">
                        <strong>Anschreiben:</strong><br />
                        {{ application.coverLetter }}
                    </p>

                   <div class="d-flex gap-2">
    <router-link
        :to="`/recruiter/applications/${application.id}`"
        class="btn btn-sm btn-outline-primary"
    >
        Details ansehen
    </router-link>

    <button
        class="btn btn-sm btn-outline-info"
        @click="updateStatus(application.id, 'Reviewed')"
    >
        Reviewed
    </button>

    <button
        class="btn btn-sm btn-outline-success"
        @click="updateStatus(application.id, 'Accepted')"
    >
        Accept
    </button>

    <button
        class="btn btn-sm btn-outline-danger"
        @click="updateStatus(application.id, 'Rejected')"
    >
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