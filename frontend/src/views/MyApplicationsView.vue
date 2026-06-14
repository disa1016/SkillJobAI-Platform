<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const applications = ref([]);
const loading = ref(true);
const error = ref("");

const getStatusClass = (status) => {
    if (status === "Pending") return "bg-warning text-dark";
    if (status === "Reviewed") return "bg-info text-dark";
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

                        <div v-if="application.status === 'Rejected'" class="alert alert-warning mt-3">
                            Deine Bewerbung wurde abgelehnt. Du kannst deine fehlenden Skills prüfen,
                            passende Kurse machen und dich danach erneut bewerben.
                        </div>

                        <div v-if="application.status === 'Accepted'" class="alert alert-success mt-3">
                            Glückwunsch! Deine Bewerbung wurde angenommen.
                        </div>

                        <div class="d-flex gap-2 mt-3 flex-wrap">
                            <router-link v-if="application.job?.id" :to="`/jobs/${application.job.id}`"
                                class="btn btn-outline-primary btn-sm">
                                Job ansehen
                            </router-link>

                            <router-link v-if="application.job?.id" :to="`/jobs/${application.job.id}/skill-gap`"
                                class="btn btn-outline-warning btn-sm">
                                Skill Gap ansehen
                            </router-link>

                            <router-link v-if="application.status === 'Rejected' && application.job?.id"
                                :to="`/jobs/${application.job.id}`" class="btn btn-primary btn-sm">
                                Erneut bewerben
                            </router-link>
                        </div>

                        <p class="text-muted mt-3 mb-0">
                            Beworben am:
                            {{ new Date(application.createdAt).toLocaleDateString("de-DE") }}
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