<script setup>
import { computed, onMounted, ref } from "vue";

import { getCourses } from "@/services/courseService";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BasePagination from "@/components/shared/BasePagination.vue";

const courses = ref([]);
const loading = ref(true);
const error = ref("");

const search = ref("");
const page = ref(1);
const pageSize = ref(9);
const totalPages = ref(1);
const totalItems = ref(0);

const hasCourses = computed(() => courses.value.length > 0);
const canGoPrevious = computed(() => page.value > 1);
const canGoNext = computed(() => page.value < totalPages.value);

const loadCourses = async () => {
    loading.value = true;
    error.value = "";

    try {
        const data = await getCourses({
            page: page.value,
            pageSize: pageSize.value,
            search: search.value,
        });

        courses.value = data.items;
        totalPages.value = data.totalPages;
        totalItems.value = data.totalItems;
    } catch {
        error.value = "Kurse konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const searchCourses = async () => {
    page.value = 1;
    await loadCourses();
};

const clearSearch = async () => {
    search.value = "";
    page.value = 1;
    await loadCourses();
};

const goToPreviousPage = async () => {
    if (!canGoPrevious.value) return;

    page.value -= 1;
    await loadCourses();
};

const goToNextPage = async () => {
    if (!canGoNext.value) return;

    page.value += 1;
    await loadCourses();
};

onMounted(loadCourses);
</script>

<template>
    <div class="container mt-4">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-4">
            <h1 class="mb-0">Courses</h1>

            <div class="d-flex flex-wrap gap-2">
                <input v-model="search" type="text" class="form-control" style="max-width: 280px"
                    placeholder="Kurs suchen..." @keyup.enter="searchCourses" />

                <button type="button" class="btn btn-primary" @click="searchCourses">
                    Suchen
                </button>

                <button type="button" class="btn btn-outline-secondary" @click="clearSearch">
                    Zurücksetzen
                </button>
            </div>
        </div>

        <BaseSpinner v-if="loading" message="Kurse werden geladen..." />

        <BaseAlert v-else-if="error" type="danger" :message="error" />

        <template v-else>
            <BaseEmptyState v-if="!hasCourses" message="Aktuell sind keine Kurse verfügbar." />

            <template v-else>
                <p class="text-muted">
                    {{ totalItems }} Kurse gefunden · Seite {{ page }} von {{ totalPages }}
                </p>

                <div class="row">
                    <div v-for="course in courses" :key="course.id" class="col-md-4 mb-3">
                        <BaseCard :title="course.title || 'Unbekannter Kurs'">
                            <p class="card-text">
                                {{ course.description || "Keine Beschreibung vorhanden." }}
                            </p>

                            <span class="badge bg-primary me-2">
                                {{ course.level || "Kein Level" }}
                            </span>

                            <span class="badge bg-secondary">
                                {{ course.category || "Keine Kategorie" }}
                            </span>

                            <template #footer>
                                <router-link :to="`/courses/${course.id}`" class="btn btn-primary w-100">
                                    Öffnen
                                </router-link>
                            </template>
                        </BaseCard>
                    </div>
                </div>

                <BasePagination :page="page" :total-pages="totalPages" :can-go-previous="canGoPrevious"
                    :can-go-next="canGoNext" @previous="goToPreviousPage" @next="goToNextPage" />
            </template>
        </template>
    </div>
</template>