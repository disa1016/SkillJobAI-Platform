<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { getCompanyById } from "@/services/companyService";

const route = useRoute();

const company = ref(null);
const jobs = ref([]);
const loading = ref(true);
const error = ref("");

const totalJobs = computed(() => jobs.value.length);
const hasJobs = computed(() => jobs.value.length > 0);

const companyStats = computed(() => [
    {
        label: "Offene Jobs",
        value: totalJobs.value,
    },
    {
        label: "Standort",
        value: company.value?.location || "Keine Angabe",
    },
    {
        label: "Seit",
        value: formatDate(company.value?.createdAt),
    },
]);

const formatDate = (date) => {
    if (!date) return "Keine Angabe";

    return new Date(date).toLocaleDateString("de-DE");
};

const loadCompany = async () => {
  loading.value = true;
  error.value = "";

  try {
    const data = await getCompanyById(route.params.id);

    company.value = data;
    jobs.value = data.jobs || [];
  } catch {
    error.value = "Firmendetails konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};

onMounted(loadCompany);
</script>

<template>
    <div class="container mt-4">
        <div v-if="loading" class="alert alert-info">
            Firmendetails werden geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else-if="company">
            <div class="card shadow-sm mb-4">
                <div class="card-body">
                    <div class="d-flex flex-wrap align-items-center gap-3 mb-3">
                        <img v-if="company.logoUrl" :src="company.logoUrl" :alt="company.name"
                            style="width: 80px; height: 80px; object-fit: contain"
                            class="border rounded p-2 bg-light" />

                        <div>
                            <h1 class="mb-1">
                                {{ company.name || "Unbekannte Firma" }}
                            </h1>

                            <p class="text-muted mb-0">
                                {{ company.location || "Kein Standort angegeben" }}
                            </p>
                        </div>
                    </div>

                    <p>
                        {{ company.description || "Keine Beschreibung vorhanden." }}
                    </p>

                    <p v-if="company.websiteUrl">
                        <strong>Website:</strong>

                        <a :href="company.websiteUrl" target="_blank" rel="noopener noreferrer">
                            {{ company.websiteUrl }}
                        </a>
                    </p>

                    <div class="row g-3 mt-3">
                        <div v-for="stat in companyStats" :key="stat.label" class="col-md-4">
                            <div class="border rounded p-3 bg-light h-100">
                                <h6 class="text-muted mb-1">
                                    {{ stat.label }}
                                </h6>

                                <h3 v-if="stat.label === 'Offene Jobs'" class="mb-0">
                                    {{ stat.value }}
                                </h3>

                                <h5 v-else class="mb-0">
                                    {{ stat.value }}
                                </h5>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="d-flex justify-content-between align-items-center mb-3">
                <h3 class="mb-0">Offene Jobs</h3>

                <span class="badge bg-primary">
                    {{ totalJobs }} Jobs
                </span>
            </div>

            <div v-if="!hasJobs" class="alert alert-info">
                Diese Firma hat aktuell keine offenen Jobs.
            </div>

            <div v-else class="row g-3">
                <div v-for="job in jobs" :key="job.id" class="col-md-6">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body">
                            <h5>
                                {{ job.title || "Ohne Titel" }}
                            </h5>

                            <p class="text-muted mb-2">
                                {{ job.location || company.location || "Kein Standort angegeben" }}
                            </p>

                            <p v-if="job.salary" class="mb-2">
                                <span class="badge bg-success">
                                    {{ job.salary }}
                                </span>
                            </p>

                            <router-link :to="`/jobs/${job.id}`" class="btn btn-primary btn-sm">
                                Job ansehen
                            </router-link>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>