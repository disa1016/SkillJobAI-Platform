<script setup>
import { computed, onMounted, ref } from "vue";
import api from "@/services/api";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";

import { APPLICATION_STATUS } from "@/constants/applicationStatus";
import { getStatusBadgeClass } from "@/utils/badge";
import { formatDate } from "@/utils/date";

const applications = ref([]);
const loading = ref(true);
const error = ref("");

const hasApplications = computed(() => applications.value.length > 0);

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

        <BaseSpinner v-if="loading" message="Bewerbungen werden geladen..." />

        <BaseAlert v-else-if="error" type="danger" :message="error" />

        <template v-else>
            <BaseEmptyState v-if="!hasApplications" message="Du hast noch keine Bewerbungen gesendet." />

            <div v-else class="row g-3">
                <div v-for="application in applications" :key="application.id" class="col-md-6">
                    <BaseCard cardClass="h-100">
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

                        <span class="badge" :class="getStatusBadgeClass(application.status)">
                            {{ application.status || "Unbekannt" }}
                        </span>

                        <BaseAlert v-if="application.status === APPLICATION_STATUS.REJECTED" type="warning" class="mt-3"
                            message="Deine Bewerbung wurde abgelehnt. Du kannst deine fehlenden Skills prüfen, passende Kurse machen und dich danach erneut bewerben." />

                        <BaseAlert v-if="application.status === APPLICATION_STATUS.ACCEPTED" type="success" class="mt-3"
                            message="Glückwunsch! Deine Bewerbung wurde angenommen." />

                        <div class="d-flex flex-wrap gap-2 mt-3">
                            <router-link v-if="application.job?.id" :to="`/jobs/${application.job.id}`"
                                class="btn btn-outline-primary btn-sm">
                                Job ansehen
                            </router-link>

                            <router-link v-if="application.job?.id" :to="`/jobs/${application.job.id}/skill-gap`"
                                class="btn btn-outline-warning btn-sm">
                                Skill Gap ansehen
                            </router-link>

                            <router-link v-if="
                                application.status === APPLICATION_STATUS.REJECTED &&
                                application.job?.id
                            " :to="`/jobs/${application.job.id}`" class="btn btn-primary btn-sm">
                                Erneut bewerben
                            </router-link>
                        </div>

                        <p class="text-muted mt-3 mb-0">
                            Beworben am:
                            {{ formatDate(application.createdAt) }}
                        </p>
                    </BaseCard>
                </div>
            </div>
        </template>
    </div>
</template>