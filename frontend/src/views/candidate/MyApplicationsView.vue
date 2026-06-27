<script setup>
import { computed, onMounted, ref } from "vue";
import api from "../../services/api";

const applications = ref([]);
const loading = ref(true);
const error = ref("");

const hasApplications = computed(() => applications.value.length > 0);

const getStatusClass = (status) => {
    const statusClasses = {
        Pending: "bg-warning text-dark",
        Reviewed: "bg-info text-dark",
        Accepted: "bg-success",
        Rejected: "bg-danger",
    };

    return statusClasses[status] || "bg-secondary";
};

const formatDate = (date) => {
    if (!date) return "Kein Datum";

    return new Date(date).toLocaleDateString("de-DE");
};

const loadApplications = async () => {
    loading.value = true;
    error.value = "";

    try {
        const { data } = await api.get("/applications/my");
        applications.value = data;
    } catch {
        error.value = "Bewerbungen konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadApplications);
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Meine Bewerbungen</h2>

        <div v-if="loading" class="alert alert-info">
            Bewerbungen werden geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <div v-if="!hasApplications" class="alert alert-light border">
                Du hast noch keine Bewerbungen gesendet.
            </div>

            <div v-else class="row g-3">
                <div v-for="application in applications" :key="application.id" class="col-md-6">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h5>
                                {{ application.job?.title || "Job gelöscht" }}
                            </h5>

                            <p class="mb-1">
                                <strong>Firma:</strong>
                                {{ application.job?.company || "Keine Firma" }}
                            </p>

                            <p class="mb-1">
                                <strong>Standort:</strong>
                                {{ application.job?.location || "Kein Standort" }}
                            </p>

                            <p class="mb-2">
                                <strong>Gehalt:</strong>
                                {{ application.job?.salary || "Kein Gehalt angegeben" }}
                            </p>

                            <span class="badge" :class="getStatusClass(application.status)">
                                {{ application.status || "Unbekannt" }}
                            </span>

                            <div v-if="application.status === 'Rejected'" class="alert alert-warning mt-3">
                                Deine Bewerbung wurde abgelehnt. Du kannst deine fehlenden Skills prüfen,
                                passende Kurse machen und dich danach erneut bewerben.
                            </div>

                            <div v-if="application.status === 'Accepted'" class="alert alert-success mt-3">
                                Glückwunsch! Deine Bewerbung wurde angenommen.
                            </div>

                            <div class="d-flex flex-wrap gap-2 mt-3">
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
                                {{ formatDate(application.createdAt) }}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>