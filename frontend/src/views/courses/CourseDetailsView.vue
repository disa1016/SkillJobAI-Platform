<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import {
  completeLessonById,
  enrollInCourse,
  getCourseById,
  getMyProgress,
} from "@/services/courseService";

const route = useRoute();

const course = ref(null);
const completedLessonIds = ref([]);

const loading = ref(true);
const enrolling = ref(false);
const completingLessonId = ref(null);

const error = ref("");
const success = ref("");

const lessons = computed(() => course.value?.lessons ?? []);
const hasLessons = computed(() => lessons.value.length > 0);

const clearMessages = () => {
  error.value = "";
  success.value = "";
};

const getErrorMessage = (err, fallback) => {
  return err.response?.data?.message || fallback;
};

const isCompleted = (lessonId) => {
  return completedLessonIds.value.includes(lessonId);
};

const loadCourse = async () => {
  course.value = await getCourseById(route.params.id);
};

const loadProgress = async () => {
  const progress = await getMyProgress();
  completedLessonIds.value = progress.map((item) => item.lessonId);
};

const loadData = async () => {
  loading.value = true;
  clearMessages();

  try {
    await Promise.all([
      loadCourse(),
      loadProgress(),
    ]);
  } catch {
    error.value = "Daten konnten nicht vollständig geladen werden.";
  } finally {
    loading.value = false;
  }
};

const enroll = async () => {
  if (!course.value?.id) return;

  enrolling.value = true;
  clearMessages();

  try {
    await enrollInCourse(course.value.id);
    success.value = "Du wurdest erfolgreich in den Kurs eingeschrieben.";
  } catch (err) {
    error.value = getErrorMessage(
      err,
      "Einschreibung konnte nicht durchgeführt werden."
    );
  } finally {
    enrolling.value = false;
  }
};

const completeLesson = async (lessonId) => {
  if (isCompleted(lessonId)) return;

  completingLessonId.value = lessonId;
  clearMessages();

  try {
    await completeLessonById(lessonId);

    completedLessonIds.value.push(lessonId);
    success.value = "Lektion wurde abgeschlossen.";
  } catch (err) {
    error.value = getErrorMessage(
      err,
      "Lektion konnte nicht abgeschlossen werden."
    );
  } finally {
    completingLessonId.value = null;
  }
};

onMounted(loadData);
</script>

<template>
  <div class="container mt-4">
    <div v-if="loading" class="alert alert-info">
      Kurs wird geladen...
    </div>

    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <template v-else-if="course">
      <div v-if="success" class="alert alert-success">
        {{ success }}
      </div>

      <h1>{{ course.title || "Unbekannter Kurs" }}</h1>

      <p class="text-muted">
        {{ course.description || "Keine Beschreibung vorhanden." }}
      </p>

      <div class="mb-3">
        <span class="badge bg-primary me-2">
          {{ course.level || "Kein Level" }}
        </span>

        <span class="badge bg-secondary">
          {{ course.category || "Keine Kategorie" }}
        </span>
      </div>

      <button
        type="button"
        class="btn btn-primary mb-4"
        :disabled="enrolling"
        @click="enroll"
      >
        {{ enrolling ? "Wird eingeschrieben..." : "In Kurs einschreiben" }}
      </button>

      <h3 class="mt-4">Lessons</h3>

      <div v-if="!hasLessons" class="alert alert-light border">
        Für diesen Kurs sind noch keine Lektionen vorhanden.
      </div>

      <div
        v-for="lesson in lessons"
        v-else
        :key="lesson.id"
        class="card shadow-sm mb-3"
      >
        <div class="card-body">
          <h5>
            {{ lesson.orderNumber }}.
            {{ lesson.title || "Unbenannte Lektion" }}

            <span
              v-if="isCompleted(lesson.id)"
              class="badge bg-success ms-2"
            >
              Abgeschlossen
            </span>
          </h5>

          <p>
            {{ lesson.content || "Kein Inhalt vorhanden." }}
          </p>

          <a
            v-if="lesson.videoUrl"
            :href="lesson.videoUrl"
            target="_blank"
            rel="noopener noreferrer"
            class="btn btn-outline-primary btn-sm me-2"
          >
            Video öffnen
          </a>

          <button
            type="button"
            class="btn btn-success btn-sm"
            :disabled="isCompleted(lesson.id) || completingLessonId === lesson.id"
            @click="completeLesson(lesson.id)"
          >
            {{
              isCompleted(lesson.id)
                ? "Bereits abgeschlossen"
                : completingLessonId === lesson.id
                  ? "Wird abgeschlossen..."
                  : "Abschließen"
            }}
          </button>
        </div>
      </div>
    </template>
  </div>
</template>