<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const companies = ref([]);
const loading = ref(true);
const error = ref("");

onMounted(async () => {
    try {
        const response = await api.get("/companies");
        companies.value = response.data;
    } catch {
        error.value = "Firmen konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="container mt-4">
        <h1 class="mb-4">Firmen</h1>

        <div v-if="loading" class="alert alert-info">
            Firmen werden geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div class="row">
            <div v-for="company in companies" :key="company.id" class="col-md-4 mb-3">
                <div class="card shadow-sm h-100">
                    <div class="card-body">
                        <h5 class="card-title">{{ company.name }}</h5>

                        <p class="text-muted mb-2">
                            {{ company.location }}
                        </p>

                        <p class="card-text">
                            {{ company.description }}
                        </p>

                        <router-link :to="`/companies/${company.id}`" class="btn btn-primary">
                            Details ansehen
                        </router-link>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>