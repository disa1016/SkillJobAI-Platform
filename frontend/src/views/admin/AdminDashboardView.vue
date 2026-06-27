<script setup>
import { computed, onMounted, ref } from "vue";
import api from "../../services/api";

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

const overviewStats = computed(() => [
    {
        label: "Users",
        value: dashboard.value?.totalUsers ?? 0,
    },
    {
        label: "Companies",
        value: dashboard.value?.totalCompanies ?? 0,
    },
    {
        label: "Jobs",
        value: dashboard.value?.totalJobs ?? 0,
    },
    {
        label: "Applications",
        value: dashboard.value?.totalApplications ?? 0,
    },
    {
        label: "Courses",
        value: dashboard.value?.totalCourses ?? 0,
    },
    {
        label: "Skills",
        value: dashboard.value?.totalSkills ?? 0,
    },
]);

const roleStats = computed(() => [
    {
        label: "Neue User heute",
        value: dashboard.value?.newUsersToday ?? 0,
        borderClass: "border-primary",
    },
    {
        label: "Neue Bewerbungen heute",
        value: dashboard.value?.newApplicationsToday ?? 0,
        borderClass: "border-success",
    },
    {
        label: "Recruiter",
        value: dashboard.value?.totalRecruiters ?? 0,
        borderClass: "border-info",
    },
    {
        label: "Admins",
        value: dashboard.value?.totalAdmins ?? 0,
        borderClass: "border-danger",
    },
]);

const loadDashboard = async () => {
    loading.value = true;
    error.value = "";

    try {
        const response = await api.get("/admin/dashboard");
        dashboard.value = response.data;
    } catch {
        error.value = "Admin Dashboard konnte nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadDashboard);
</script>

<template>
    <div class="container py-4">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
            <h2 class="mb-0">Admin Dashboard</h2>

            <router-link to="/admin/users" class="btn btn-primary">
                User Management öffnen
            </router-link>
        </div>

        <div v-if="loading" class="alert alert-info">
            Dashboard wird geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else-if="dashboard">
            <section class="mb-4">
                <h5 class="mb-3">Übersicht</h5>

                <div class="row g-3">
                    <div v-for="stat in overviewStats" :key="stat.label" class="col-md-4">
                        <div class="card shadow-sm h-100">
                            <div class="card-body">
                                <h6 class="text-muted">{{ stat.label }}</h6>
                                <h2 class="mb-0">{{ stat.value }}</h2>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <section>
                <h5 class="mb-3">Heute & Rollen</h5>

                <div class="row g-3">
                    <div v-for="stat in roleStats" :key="stat.label" class="col-md-3">
                        <div class="card shadow-sm h-100" :class="stat.borderClass">
                            <div class="card-body">
                                <h6 class="text-muted">{{ stat.label }}</h6>
                                <h2 class="mb-0">{{ stat.value }}</h2>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </template>
    </div>
</template>