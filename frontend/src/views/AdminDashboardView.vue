<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const dashboard = ref(null);
const loading = ref(true);
const error = ref("");

onMounted(async () => {
    try {
        const response = await api.get("/admin/dashboard");
        dashboard.value = response.data;
    } catch {
        error.value = "Admin Dashboard konnte nicht geladen werden.";
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Admin Dashboard</h2>

        <div v-if="loading" class="alert alert-info">
            Dashboard wird geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="dashboard">
            <h5 class="mb-3">Übersicht</h5>

            <div class="row g-3 mb-4">
                <div class="col-md-4">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h6> Users</h6>
                            <h2>{{ dashboard.totalUsers }}</h2>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h6> Companies</h6>
                            <h2>{{ dashboard.totalCompanies }}</h2>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h6>Jobs</h6>
                            <h2>{{ dashboard.totalJobs }}</h2>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h6> Applications</h6>
                            <h2>{{ dashboard.totalApplications }}</h2>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h6> Courses</h6>
                            <h2>{{ dashboard.totalCourses }}</h2>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h6> Skills</h6>
                            <h2>{{ dashboard.totalSkills }}</h2>
                        </div>
                    </div>
                </div>
            </div>

            <h5 class="mb-3">Heute & Rollen</h5>

            <div class="row g-3">
                <div class="col-md-3">
                    <div class="card shadow-sm h-100 border-primary">
                        <div class="card-body">
                            <h6> Neue User heute</h6>
                            <h2>{{ dashboard.newUsersToday }}</h2>
                        </div>
                    </div>
                </div>

                <div class="col-md-3">
                    <div class="card shadow-sm h-100 border-success">
                        <div class="card-body">
                            <h6> Neue Bewerbungen heute</h6>
                            <h2>{{ dashboard.newApplicationsToday }}</h2>
                        </div>
                    </div>
                </div>

                <div class="col-md-3">
                    <div class="card shadow-sm h-100 border-info">
                        <div class="card-body">
                            <h6> Recruiter</h6>
                            <h2>{{ dashboard.totalRecruiters }}</h2>
                        </div>
                    </div>
                </div>

                <div class="col-md-3">
                    <div class="card shadow-sm h-100 border-danger">
                        <div class="card-body">
                            <h6> Admins</h6>
                            <h2>{{ dashboard.totalAdmins }}</h2>
                        </div>
                    </div>
                </div>
            </div>

            <div class="mt-4">
                <router-link to="/admin/users" class="btn btn-primary">
                    User Management öffnen
                </router-link>
            </div>
        </div>
    </div>
</template>