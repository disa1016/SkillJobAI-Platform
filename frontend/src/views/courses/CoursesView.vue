<script setup>
import { computed, onMounted, ref } from "vue";
import { getCourses } from "@/services/courseService";
import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";

const courses = ref([]);
const loading = ref(true);
const error = ref("");

const hasCourses = computed(() => courses.value.length > 0);

const loadCourses = async () => {
    loading.value = true;
    error.value = "";

    try {
        courses.value = await getCourses();
    } catch {
        error.value = "Kurse konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadCourses);
</script>

<template>
    <div class="container mt-4">
        <h1 class="mb-4">Courses</h1>

        <BaseSpinner v-if="loading" message="Kurse werden geladen..." />

        <BaseAlert v-else-if="error" type="danger" :message="error" />

        <template v-else>
            <BaseEmptyState v-if="!hasCourses" message="Aktuell sind keine Kurse verfügbar." />

            <div v-else class="row">
                <div v-for="course in courses" :key="course.id" class="col-md-4 mb-3">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h5 class="card-title">
                                {{ course.title || "Unbekannter Kurs" }}
                            </h5>

                            <p class="card-text">
                                {{ course.description || "Keine Beschreibung vorhanden." }}
                            </p>

                            <span class="badge bg-primary me-2">
                                {{ course.level || "Kein Level" }}
                            </span>

                            <span class="badge bg-secondary">
                                {{ course.category || "Keine Kategorie" }}
                            </span>
                        </div>

                        <div class="card-footer bg-white border-0">
                            <router-link :to="`/courses/${course.id}`" class="btn btn-primary w-100">
                                Öffnen
                            </router-link>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>