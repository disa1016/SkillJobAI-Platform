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

        <div v-if="dashboard" class="row g-3">
            <div class="col-md-4">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Users</h6>
                        <h2>{{ dashboard.totalUsers }}</h2>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Companies</h6>
                        <h2>{{ dashboard.totalCompanies }}</h2>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Jobs</h6>
                        <h2>{{ dashboard.totalJobs }}</h2>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Applications</h6>
                        <h2>{{ dashboard.totalApplications }}</h2>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Courses</h6>
                        <h2>{{ dashboard.totalCourses }}</h2>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h6>Skills</h6>
                        <h2>{{ dashboard.totalSkills }}</h2>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>