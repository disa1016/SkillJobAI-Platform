<script setup>
import { computed, onMounted, ref } from "vue";
import { getJobs } from "@/services/jobService";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseCard from "@/components/shared/BaseCard.vue";

const jobs = ref([]);
const loading = ref(true);
const error = ref("");

const hasJobs = computed(() => jobs.value.length > 0);

const loadJobs = async () => {
    loading.value = true;
    error.value = "";

    try {
        jobs.value = await getJobs();
    } catch {
        error.value = "Jobs konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadJobs);
</script>

<template>
    <div class="container mt-4">
        <h1 class="mb-4">Jobs</h1>

        <BaseSpinner v-if="loading" message="Jobs werden geladen..." />

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <BaseEmptyState v-if="!hasJobs" message="Aktuell sind keine Jobs verfügbar." />

            <div v-else class="row">
                <div v-for="job in jobs" :key="job.id" class="col-md-4 mb-3">
                    <BaseCard :title="job.title || 'Ohne Titel'">
                        <p v-if="job.company" class="mb-2 text-muted">
                            Firma:
                            <router-link :to="`/companies/${job.company.id}`" class="text-decoration-none fw-semibold">
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
                    </BaseCard>>
                </div>
            </div>
        </template>
    </div>
</template>