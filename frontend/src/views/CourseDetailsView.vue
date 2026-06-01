<script setup>
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import api from "../services/api";

const route = useRoute();

const course = ref(null);
const completedLessonIds = ref([]);

const loading = ref(true);
const error = ref("");
const success = ref("");

const enroll = async () => {
    error.value = "";
    success.value = "";

    try {
        await api.post("/enrollments", {
            courseId: course.value.id,
        });
        success.value = "Du wurdest erfolgreich in den Kurs eingeschrieben.";
    } catch (err) {
        if (err.response?.data?.message) {
            error.value = err.response.data.message;
        } else {
            error.value = "Einschreibung konnte nicht durchgeführt werden.";
        }
    }
};
const loadCourse = async () => {
    const response = await api.get(`/courses/${route.params.id}`);
    course.value = response.data;
};

const loadProgress = async () => {
    const response = await api.get("/progress/my");
    completedLessonIds.value = response.data.map((item) => item.lessonId);
};

const isCompleted = (lessonId) => {
    return completedLessonIds.value.includes(lessonId);
};

const completeLesson = async (lessonId) => {
    error.value = "";
    success.value = "";

    try {
        await api.post("/progress/complete", {
            lessonId: lessonId,
        });

        completedLessonIds.value.push(lessonId);
        success.value = "Lektion wurde abgeschlossen.";
    } catch (err) {
        if (err.response?.data?.message) {
            error.value = err.response.data.message;
        } else {
            error.value = "Lektion konnte nicht abgeschlossen werden.";
        }
    }
};

onMounted(async () => {
    try {
        await loadCourse();
        await loadProgress();
    } catch {
        error.value = "Daten konnten nicht vollständig geladen werden.";
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="container mt-4">
        <div v-if="loading" class="alert alert-info">
            Kurs wird geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div v-if="course">
            <h1>{{ course.title }}</h1>
            <p class="text-muted">{{ course.description }}</p>

            <div class="mb-3">
                <span class="badge bg-primary me-2">{{ course.level }}</span>
                <span class="badge bg-secondary">{{ course.category }}</span>
            </div>

            <button class="btn btn-primary mb-4" @click="enroll">
                In Kurs einschreiben
            </button>

            <h3 class="mt-4">Lessons</h3>

            <div v-for="lesson in course.lessons" :key="lesson.id" class="card shadow-sm mb-3">
                <div class="card-body">
                    <h5>
                        {{ lesson.orderNumber }}. {{ lesson.title }}
                        <span v-if="isCompleted(lesson.id)" class="badge bg-success ms-2">
                            Abgeschlossen
                        </span>
                    </h5>

                    <p>{{ lesson.content }}</p>

                    <a v-if="lesson.videoUrl" :href="lesson.videoUrl" target="_blank"
                        class="btn btn-outline-primary btn-sm me-2">
                        Video öffnen
                    </a>

                    <button class="btn btn-success btn-sm" :disabled="isCompleted(lesson.id)"
                        @click="completeLesson(lesson.id)">
                        {{ isCompleted(lesson.id) ? "Bereits abgeschlossen" : "Abschließen" }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>