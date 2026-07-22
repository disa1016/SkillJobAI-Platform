<script setup>
import { computed, onMounted, ref } from "vue";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

import { getAdminDashboard } from "@/services/adminService";

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

const overviewStats = computed(() => [
    { label: "Benutzer", value: dashboard.value?.totalUsers ?? 0, icon: "bi-people" },
    { label: "Unternehmen", value: dashboard.value?.totalCompanies ?? 0, icon: "bi-buildings" },
    { label: "Stellenangebote", value: dashboard.value?.totalJobs ?? 0, icon: "bi-briefcase" },
    { label: "Bewerbungen", value: dashboard.value?.totalApplications ?? 0, icon: "bi-file-earmark-person" },
    { label: "Kurse", value: dashboard.value?.totalCourses ?? 0, icon: "bi-journal-bookmark" },
    { label: "Skills", value: dashboard.value?.totalSkills ?? 0, icon: "bi-lightbulb" },
]);

const roleStats = computed(() => [
    { label: "Neue Benutzer heute", value: dashboard.value?.newUsersToday ?? 0, icon: "bi-person-plus" },
    { label: "Neue Bewerbungen heute", value: dashboard.value?.newApplicationsToday ?? 0, icon: "bi-file-earmark-plus" },
    { label: "Recruiter", value: dashboard.value?.totalRecruiters ?? 0, icon: "bi-person-workspace" },
    { label: "Admins", value: dashboard.value?.totalAdmins ?? 0, icon: "bi-shield-check" },
]);

const loadDashboard = async () => {
    loading.value = true;
    error.value = "";

    try {
        dashboard.value = await getAdminDashboard();
    } catch {
        error.value = "Admin-Dashboard konnte nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadDashboard);
</script>

<template>
    <main class="container py-4">
        <PageHeader title="Admin-Dashboard"
            description="Zentrale Übersicht über Benutzer, Unternehmen und Plattformaktivitäten.">
            <template #actions>
                <router-link to="/admin/users" class="btn btn-primary">
                    <i class="bi bi-people me-2" aria-hidden="true"></i>
                    Benutzer verwalten
                </router-link>
            </template>
        </PageHeader>

        <BaseSpinner v-if="loading" message="Dashboard wird geladen..." />

        <BaseAlert v-else-if="error" type="danger" :message="error" />

        <template v-else-if="dashboard">
            <section class="mb-5" aria-labelledby="admin-overview-heading">
                <div class="mb-3">
                    <h2 id="admin-overview-heading" class="h5 mb-1">Übersicht</h2>
                    <p class="text-body-secondary mb-0">Aktuelle Gesamtzahlen der Plattform.</p>
                </div>

                <div class="row g-3">
                    <div v-for="stat in overviewStats" :key="stat.label" class="col-12 col-sm-6 col-lg-4">
                        <BaseCard>
                            <div class="d-flex justify-content-between align-items-start gap-3">
                                <div>
                                    <p class="text-body-secondary mb-2">{{ stat.label }}</p>
                                    <p class="h2 mb-0">{{ stat.value }}</p>
                                </div>

                                <i class="bi fs-3 text-primary" :class="stat.icon" aria-hidden="true"></i>
                            </div>
                        </BaseCard>
                    </div>
                </div>
            </section>

            <section aria-labelledby="admin-today-heading">
                <div class="mb-3">
                    <h2 id="admin-today-heading" class="h5 mb-1">Heute und Rollen</h2>
                    <p class="text-body-secondary mb-0">Neue Aktivitäten und Verteilung wichtiger Rollen.</p>
                </div>

                <div class="row g-3">
                    <div v-for="stat in roleStats" :key="stat.label" class="col-12 col-sm-6 col-xl-3">
                        <BaseCard>
                            <div class="d-flex justify-content-between align-items-start gap-3">
                                <div>
                                    <p class="text-body-secondary mb-2">{{ stat.label }}</p>
                                    <p class="h2 mb-0">{{ stat.value }}</p>
                                </div>

                                <i class="bi fs-3 text-primary" :class="stat.icon" aria-hidden="true"></i>
                            </div>
                        </BaseCard>
                    </div>
                </div>
            </section>
        </template>

        <BaseEmptyState v-else title="Keine Dashboard-Daten"
            message="Es konnten keine Admin-Dashboard-Daten gefunden werden." icon="bi-speedometer2" />
    </main>
</template>
