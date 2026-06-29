<script setup>
import { computed, onMounted, ref, watch } from "vue";
import api from "@/services/api";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";

import { APPLICATION_STATUS } from "@/constants/applicationStatus";
import { getMatchBadgeClass, getStatusBadgeClass } from "@/utils/badge";

const jobs = ref([]);
const selectedJobId = ref("");
const applications = ref([]);

const jobsLoading = ref(true);
const applicationsLoading = ref(false);

const error = ref("");
const success = ref("");

const page = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);
const totalItems = ref(0);
const search = ref("");
const selectedStatus = ref("");

const backendUrl = computed(() => {
    const baseUrl = api.defaults.baseURL || "";
    return baseUrl.replace("/api", "");
});

const hasApplications = computed(() => applications.value.length > 0);
const hasSelectedJob = computed(() => Boolean(selectedJobId.value));
const canGoPrevious = computed(() => page.value > 1);
const canGoNext = computed(() => page.value < totalPages.value);

const selectedJob = computed(() =>
    jobs.value.find((job) => job.id === Number(selectedJobId.value))
);

const getFileUrl = (fileUrl) => {
    if (!fileUrl) return "";
    if (fileUrl.startsWith("http")) return fileUrl;

    return `${backendUrl.value}${fileUrl}`;
};

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const getMatchPercentage = (score) => {
    const value = Number(score) || 0;
    return Math.min(Math.max(value, 0), 100);
};

const hasApplicationFiles = (application) => {
    return Boolean(
        application.cvFileUrl ||
        application.certificateFileUrl ||
        application.portfolioFileUrl
    );
};

const hasSkills = (application) => {
    return Boolean(
        application.matchedSkills?.length ||
        application.missingSkills?.length
    );
};

const loadJobs = async () => {
    jobsLoading.value = true;
    clearMessages();

    try {
       const { data } = await api.get("/jobs", {
  params: {
    page: 1,
    pageSize: 50,
  },
});

console.log("Jobs response:", data);

jobs.value = data.items || data;

    } catch {
        error.value = "Jobs konnten nicht geladen werden.";
    } finally {
        jobsLoading.value = false;
    }
};

const loadApplications = async () => {
    if (!selectedJobId.value) return;

    applicationsLoading.value = true;
    applications.value = [];
    clearMessages();

    try {
        const { data } = await api.get(`/applications/job/${selectedJobId.value}`, {
            params: {
                page: page.value,
                pageSize: pageSize.value,
                search: search.value,
                status: selectedStatus.value,
            },
        });

        applications.value = data.items;
        totalPages.value = data.totalPages;
        totalItems.value = data.totalItems;
    } catch {
        error.value = "Bewerbungen konnten nicht geladen werden.";
    } finally {
        applicationsLoading.value = false;
    }
};

const searchApplications = async () => {
    page.value = 1;
    await loadApplications();
};

const clearFilters = async () => {
    search.value = "";
    selectedStatus.value = "";
    page.value = 1;
    await loadApplications();
};

const goToPreviousPage = async () => {
    if (!canGoPrevious.value) return;

    page.value -= 1;
    await loadApplications();
};

const goToNextPage = async () => {
    if (!canGoNext.value) return;

    page.value += 1;
    await loadApplications();
};

const updateStatus = async (applicationId, status) => {
    clearMessages();

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

watch(selectedJobId, () => {
    applications.value = [];
    success.value = "";
    error.value = "";
    page.value = 1;
    totalPages.value = 1;
    totalItems.value = 0;
});

onMounted(loadJobs);
</script>


<template>
    <div class="container py-4">
        <h2 class="mb-4">Recruiter Applications</h2>

        <BaseAlert v-if="error" type="danger" :message="error" />

        <BaseAlert v-if="success" type="success" :message="success" />

        <BaseCard class="mb-4">
            <h5>Job auswählen</h5>

            <div class="d-flex flex-wrap gap-2">
                <select v-model="selectedJobId" class="form-select" :disabled="jobsLoading">
                    <option value="">
                        {{ jobsLoading ? "Jobs werden geladen..." : "Job auswählen" }}
                    </option>

                    <option v-for="job in jobs" :key="job.id" :value="String(job.id)">
                        {{ job.title || "Ohne Titel" }} -
                        {{ job.company?.name || "Keine Firma" }}
                    </option>
                </select>

                <button type="button" class="btn btn-primary" :disabled="!hasSelectedJob || applicationsLoading"
                    @click="loadApplications">
                    {{ applicationsLoading ? "Lädt..." : "Bewerbungen laden" }}
                </button>
            </div>

            <p v-if="selectedJob" class="text-muted mt-2 mb-0">
                Ausgewählt:
                {{ selectedJob.title || "Ohne Titel" }}
            </p>

            <p v-if="!jobsLoading && jobs.length === 0" class="text-muted mt-2 mb-0">
                Keine Jobs gefunden.
            </p>
        </BaseCard>

        <BaseCard v-if="hasSelectedJob" class="mb-4">
            <h5>Filter</h5>

            <div class="d-flex flex-wrap gap-2">
                <input v-model="search" type="text" class="form-control" style="max-width: 280px"
                    placeholder="Kandidat suchen..." @keyup.enter="searchApplications" />

                <select v-model="selectedStatus" class="form-select" style="max-width: 220px">
                    <option value="">Alle Status</option>
                    <option :value="APPLICATION_STATUS.PENDING">Pending</option>
                    <option :value="APPLICATION_STATUS.REVIEWED">Reviewed</option>
                    <option :value="APPLICATION_STATUS.ACCEPTED">Accepted</option>
                    <option :value="APPLICATION_STATUS.REJECTED">Rejected</option>
                </select>

                <button type="button" class="btn btn-primary" :disabled="applicationsLoading"
                    @click="searchApplications">
                    Suchen
                </button>

                <button type="button" class="btn btn-outline-secondary" :disabled="applicationsLoading"
                    @click="clearFilters">
                    Zurücksetzen
                </button>
            </div>
        </BaseCard>

        <BaseSpinner v-if="applicationsLoading" message="Bewerbungen werden geladen..." />

        <template v-else>
            <BaseCard v-if="hasApplications">
                <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <h5 class="mb-0">Bewerbungen Ranking</h5>

                    <span class="text-muted">
                        {{ totalItems }} Bewerbungen · Seite {{ page }} von {{ totalPages }}
                    </span>
                </div>

                <div v-for="(application, index) in applications" :key="application.id" class="border rounded p-3 mb-3">
                    <div class="d-flex justify-content-between align-items-start gap-3 mb-2">
                        <div>
                            <h5 class="mb-1">
                                #{{ (page - 1) * pageSize + index + 1 }}
                                {{ application.candidate?.fullName || "Unbekannter Kandidat" }}
                            </h5>

                            <p class="text-muted mb-1">
                                {{ application.candidate?.email || "Keine E-Mail" }}
                            </p>
                        </div>

                        <span class="badge fs-6" :class="getMatchBadgeClass(application.matchPercentage)">
                            Match {{ getMatchPercentage(application.matchPercentage) }}%
                        </span>
                    </div>

                    <div class="progress mb-3" style="height: 24px">
                        <div class="progress-bar" role="progressbar"
                            :class="getMatchBadgeClass(application.matchPercentage)"
                            :style="`width: ${getMatchPercentage(application.matchPercentage)}%`">
                            {{ getMatchPercentage(application.matchPercentage) }}%
                        </div>
                    </div>

                    <p>
                        <strong>Status:</strong>

                        <span class="badge" :class="getStatusBadgeClass(application.status)">
                            {{ application.status || "Unbekannt" }}
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

                            <span v-if="!hasApplicationFiles(application)" class="text-muted">
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
                                :key="`${course.id}-${course.skill}`" :to="`/courses/${course.id}`"
                                class="badge bg-primary me-2 mb-2 text-decoration-none">
                                {{ course.title }} für {{ course.skill }}
                            </router-link>
                        </div>
                    </div>

                    <BaseAlert v-if="!hasSkills(application)" type="warning"
                        message="Für diesen Job wurden noch keine Skills hinterlegt." />

                    <p v-if="application.coverLetter">
                        <strong>Anschreiben:</strong>
                        <br />
                        {{ application.coverLetter }}
                    </p>

                    <div class="d-flex flex-wrap gap-2">
                        <router-link :to="`/recruiter/applications/${application.id}`"
                            class="btn btn-sm btn-outline-primary">
                            Details ansehen
                        </router-link>

                        <button type="button" class="btn btn-sm btn-outline-info"
                            @click="updateStatus(application.id, APPLICATION_STATUS.REVIEWED)">
                            Reviewed
                        </button>

                        <button type="button" class="btn btn-sm btn-outline-success"
                            @click="updateStatus(application.id, APPLICATION_STATUS.ACCEPTED)">
                            Accept
                        </button>

                        <button type="button" class="btn btn-sm btn-outline-danger"
                            @click="updateStatus(application.id, APPLICATION_STATUS.REJECTED)">
                            Reject
                        </button>
                    </div>
                </div>

                <div class="d-flex justify-content-center align-items-center gap-2 mt-4">
                    <button type="button" class="btn btn-outline-primary" :disabled="!canGoPrevious"
                        @click="goToPreviousPage">
                        Zurück
                    </button>

                    <span class="text-muted">
                        Seite {{ page }} / {{ totalPages }}
                    </span>

                    <button type="button" class="btn btn-outline-primary" :disabled="!canGoNext" @click="goToNextPage">
                        Weiter
                    </button>
                </div>
            </BaseCard>

            <BaseEmptyState v-if="hasSelectedJob && !hasApplications"
                message="Für diesen Job gibt es noch keine Bewerbungen." />
        </template>
    </div>
</template>