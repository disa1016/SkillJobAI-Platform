<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const applications = ref([]);
const loading = ref(true);
const error = ref("");

const getStatusClass = (status) => {
    if (status === "Pending") return "bg-warning";
    if (status === "Reviewed") return "bg-info";
    if (status === "Accepted") return "bg-success";
    if (status === "Rejected") return "bg-danger";
    return "bg-secondary";
};

onMounted(async () => {
    try {
        const response = await api.get("/applications/my");
        applications.value = response.data;
    } catch {
        error.value = "Bewerbungen konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Meine Bewerbungen</h2>

        <div v-if="loading" class="alert alert-info">
            Bewerbungen werden geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="applications.length > 0" class="row g-3">
            <div v-for="application in applications" :key="application.id" class="col-md-6">
                <div class="card shadow-sm h-100">
                    <div class="card-body">
                        <h5>{{ application.job?.title }}</h5>

                        <p class="mb-1">
                            <strong>Firma:</strong>
                            {{ application.job?.company || "Keine Firma" }}
                        </p>

                        <p class="mb-1">
                            <strong>Standort:</strong>
                            {{ application.job?.location }}
                        </p>

                        <p class="mb-2">
                            <strong>Gehalt:</strong>
                            {{ application.job?.salary }}
                        </p>

                        <span class="badge" :class="getStatusClass(application.status)">
                            {{ application.status }}
                        </span>

                        <p class="text-muted mt-3 mb-0">
                            Beworben am: {{ new Date(application.createdAt).toLocaleDateString() }}
                        </p>
                    </div>
                </div>
            </div>
        </div>

        <p v-if="!loading && applications.length === 0" class="text-muted">
            Du hast noch keine Bewerbungen gesendet.
        </p>
    </div>
</template>