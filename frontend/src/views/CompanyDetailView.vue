<script setup>
import { ref, onMounted, computed } from "vue";
import { useRoute } from "vue-router";
import api from "../services/api";

const route = useRoute();

const company = ref(null);
const jobs = ref([]);
const loading = ref(true);
const error = ref("");

const totalJobs = computed(() => jobs.value.length);

onMounted(async () => {
    try {
        const companyId = route.params.id;

        const response = await api.get(`/companies/${companyId}`);

        company.value = response.data;
        jobs.value = response.data.jobs || [];
    } catch (err) {
        console.error(err);
        error.value = "Firmendetails konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="container mt-4">
        <div v-if="loading" class="alert alert-info">
            Firmendetails werden geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-else-if="company">
            <div class="card shadow-sm mb-4">
                <div class="card-body">
                    <div class="d-flex align-items-center gap-3 mb-3">
                        <img v-if="company.logoUrl" :src="company.logoUrl" :alt="company.name"
                            style="width: 80px; height: 80px; object-fit: contain;"
                            class="border rounded p-2 bg-light" />

                        <div>
                            <h1 class="mb-1">{{ company.name }}</h1>

                            <p class="text-muted mb-0">
                                {{ company.location || "Kein Standort angegeben" }}
                            </p>
                        </div>
                    </div>

                    <p v-if="company.description">
                        {{ company.description }}
                    </p>

                    <p v-else class="text-muted">
                        Keine Beschreibung vorhanden.
                    </p>

                    <p v-if="company.websiteUrl">
                        <strong>Website:</strong>
                        <a :href="company.websiteUrl" target="_blank" rel="noopener noreferrer">
                            {{ company.websiteUrl }}
                        </a>
                    </p>

                    <div class="row g-3 mt-3">
                        <div class="col-md-4">
                            <div class="border rounded p-3 bg-light">
                                <h6 class="text-muted mb-1">Offene Jobs</h6>
                                <h3 class="mb-0">{{ totalJobs }}</h3>
                            </div>
                        </div>

                        <div class="col-md-4">
                            <div class="border rounded p-3 bg-light">
                                <h6 class="text-muted mb-1">Standort</h6>
                                <h5 class="mb-0">
                                    {{ company.location || "Keine Angabe" }}
                                </h5>
                            </div>
                        </div>

                        <div class="col-md-4">
                            <div class="border rounded p-3 bg-light">
                                <h6 class="text-muted mb-1">Seit</h6>
                                <h5 class="mb-0">
                                    {{
                                        company.createdAt
                                            ? new Date(company.createdAt).toLocaleDateString("de-DE")
                                            : "Keine Angabe"
                                    }}
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

            <div v-if="jobs.length === 0" class="alert alert-info">
                Diese Firma hat aktuell keine offenen Jobs.
            </div>

            <div class="row g-3">
                <div v-for="job in jobs" :key="job.id" class="col-md-6">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body">
                            <h5>{{ job.title }}</h5>

                            <p class="text-muted mb-2">
                                {{ job.location || company.location }}
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
        </div>
    </div>
</template>