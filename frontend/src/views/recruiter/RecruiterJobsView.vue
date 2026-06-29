<script setup>
import { computed, onMounted, ref } from "vue";
import api from "@/services/api";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";

const jobs = ref([]);
const loading = ref(true);
const error = ref("");
const success = ref("");

const page = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);
const totalItems = ref(0);
const search = ref("");

const hasJobs = computed(() => jobs.value.length > 0);
const canGoPrevious = computed(() => page.value > 1);
const canGoNext = computed(() => page.value < totalPages.value);

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const loadJobs = async () => {
    loading.value = true;
    error.value = "";

    try {
        const { data } = await api.get("/jobs", {
            params: {
                page: page.value,
                pageSize: pageSize.value,
                search: search.value,
            },
        });

        jobs.value = data.items;
        totalPages.value = data.totalPages;
        totalItems.value = data.totalItems;
    } catch {
        error.value = "Jobs konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const searchJobs = async () => {
    page.value = 1;
    await loadJobs();
};

const clearSearch = async () => {
    search.value = "";
    page.value = 1;
    await loadJobs();
};

const goToPreviousPage = async () => {
    if (!canGoPrevious.value) return;

    page.value -= 1;
    await loadJobs();
};

const goToNextPage = async () => {
    if (!canGoNext.value) return;

    page.value += 1;
    await loadJobs();
};

const deleteJob = async (id) => {
    if (!confirm("Möchtest du diesen Job wirklich löschen?")) return;

    clearMessages();

    try {
        await api.delete(`/jobs/${id}`);

        if (jobs.value.length === 1 && page.value > 1) {
            page.value -= 1;
        }

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

        <div class="d-flex flex-wrap gap-2 mb-3">
            <input v-model="search" type="text" class="form-control" style="max-width: 320px"
                placeholder="Job suchen..." @keyup.enter="searchJobs" />

            <button type="button" class="btn btn-primary" @click="searchJobs">
                Suchen
            </button>

            <button type="button" class="btn btn-outline-secondary" @click="clearSearch">
                Zurücksetzen
            </button>
        </div>

        <BaseSpinner v-if="loading" message="Jobs werden geladen..." />

        <BaseAlert v-else-if="error" type="danger" :message="error" />

        <template v-else>
            <BaseAlert v-if="success" type="success" :message="success" />

            <BaseEmptyState v-if="!hasJobs" message="Noch keine Jobs vorhanden." />

            <template v-else>
                <p class="text-muted">
                    {{ totalItems }} Jobs gefunden · Seite {{ page }} von {{ totalPages }}
                </p>

                <div class="row g-3">
                    <div v-for="job in jobs" :key="job.id" class="col-md-6">
                        <BaseCard :title="job.title || 'Ohne Titel'" cardClass="h-100">
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

                            <template #footer>
                                <div class="d-flex flex-wrap gap-2">
                                    <router-link :to="`/jobs/${job.id}`" class="btn btn-sm btn-outline-secondary">
                                        Anzeigen
                                    </router-link>

                                    <router-link :to="`/recruiter/jobs/edit/${job.id}`"
                                        class="btn btn-sm btn-outline-primary">
                                        Bearbeiten
                                    </router-link>

                                    <button type="button" class="btn btn-sm btn-outline-danger"
                                        @click="deleteJob(job.id)">
                                        Löschen
                                    </button>
                                </div>
                            </template>
                        </BaseCard>
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
            </template>
        </template>
    </div>
</template>