<script setup>
import { ref, onMounted } from "vue";
import { useRoute } from "vue-router";
import api from "../services/api";

const route = useRoute();

const company = ref(null);
const jobs = ref([]);
const loading = ref(true);
const error = ref("");

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

                    <h1>{{ company.name }}</h1>

                    <p class="text-muted">
                        {{ company.location }}
                    </p>

                    <p>
                        {{ company.description }}
                    </p>

                    <p v-if="company.websiteUrl">
                        <strong>Website:</strong>
                        <a :href="company.websiteUrl" target="_blank">
                            {{ company.websiteUrl }}
                        </a>
                    </p>

                </div>
            </div>

            <h3>Offene Jobs</h3>

            <div v-for="job in jobs" :key="job.id" class="card mb-3">
                <div class="card-body">

                    <h5>{{ job.title }}</h5>

                    <p>{{ job.location }}</p>

                    <p v-if="job.salary">
                        Gehalt: {{ job.salary }}
                    </p>

                    <router-link :to="`/jobs/${job.id}`" class="btn btn-primary btn-sm">
                        Job ansehen
                    </router-link>

                </div>
            </div>

        </div>

    </div>
</template>