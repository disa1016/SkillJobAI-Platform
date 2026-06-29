<script setup>
import { computed, onMounted, ref } from "vue";

import { getJobs } from "@/services/jobService";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";

const jobs = ref([]);
const loading = ref(true);
const error = ref("");

const search = ref("");
const page = ref(1);
const pageSize = ref(9);
const totalPages = ref(1);
const totalItems = ref(0);

const hasJobs = computed(() => jobs.value.length > 0);
const canGoPrevious = computed(() => page.value > 1);
const canGoNext = computed(() => page.value < totalPages.value);

const loadJobs = async () => {
    loading.value = true;
    error.value = "";

    try {
        const data = await getJobs({
            page: page.value,
            pageSize: pageSize.value,
            search: search.value,
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

onMounted(loadJobs);
</script>

<template>
    <div class="container mt-4">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-4">
            <h1 class="mb-0">Jobs</h1>

            <div class="d-flex flex-wrap gap-2">
                <input v-model="search" type="text" class="form-control" style="max-width: 280px"
                    placeholder="Job suchen..." @keyup.enter="searchJobs" />

                <button type="button" class="btn btn-primary" @click="searchJobs">
                    Suchen
                </button>

                <button type="button" class="btn btn-outline-secondary" @click="clearSearch">
                    Zurücksetzen
                </button>
            </div>
        </div>

        <BaseSpinner v-if="loading" message="Jobs werden geladen..." />

        <BaseAlert v-else-if="error" type="danger" :message="error" />

        <template v-else>
            <BaseEmptyState v-if="!hasJobs" message="Aktuell sind keine Jobs verfügbar." />

            <template v-else>
                <p class="text-muted">
                    {{ totalItems }} Jobs gefunden · Seite {{ page }} von {{ totalPages }}
                </p>

                <div class="row">
                    <div v-for="job in jobs" :key="job.id" class="col-md-4 mb-3">
                        <BaseCard :title="job.title || 'Ohne Titel'">
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

                            <template #footer>
                                <router-link :to="`/jobs/${job.id}`" class="btn btn-primary w-100">
                                    Details
                                </router-link>
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