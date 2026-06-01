<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const courses = ref([]);
const loading = ref(true);
const error = ref("");

onMounted(async () => {
    try {
        const response = await api.get("/courses");
        courses.value = response.data;
    } catch {
        error.value = "Kurse konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="container mt-4">
        <h1 class="mb-4">Courses</h1>

        <div v-if="loading" class="alert alert-info">
            Kurse werden geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div class="row">
            <div v-for="course in courses" :key="course.id" class="col-md-4 mb-3">
                <div class="card shadow-sm h-100">
                    <div class="card-body">

                        <h5 class="card-title">
                            {{ course.title }}
                        </h5>

                        <p class="card-text">
                            {{ course.description }}
                        </p>

                        <span class="badge bg-primary me-2">
                            {{ course.level }}
                        </span>

                        <span class="badge bg-secondary">
                            {{ course.category }}
                        </span>

                        <router-link :to="`/courses/${course.id}`" class="btn btn-primary mt-3 d-block">
                            Öffnen
                        </router-link>

                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped></style>