<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const user = JSON.parse(localStorage.getItem("user"));

const applications = ref([]);
const enrollments = ref([]);
const progress = ref([]);

const loading = ref(true);
const error = ref("");
const certificateError = ref("");

onMounted(async () => {
  try {
    const applicationsResponse = await api.get("/applications/my");
    const enrollmentsResponse = await api.get("/enrollments/my");
    const progressResponse = await api.get("/progress/my");

    applications.value = applicationsResponse.data;
    enrollments.value = enrollmentsResponse.data;
    progress.value = progressResponse.data;
  } catch {
    error.value = "Profil-Daten konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
});

const downloadCertificate = async (courseId, courseTitle) => {
  certificateError.value = "";

  try {
    const response = await api.get(`/certificates/course/${courseId}`, {
      responseType: "blob",
    });

    const blob = new Blob([response.data], {
      type: "application/pdf",
    });

    const url = window.URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.href = url;
    link.download = `certificate-${courseTitle}.pdf`;
    link.click();

    window.URL.revokeObjectURL(url);
  } catch (err) {
    if (err.response?.data instanceof Blob) {
      const text = await err.response.data.text();
      const data = JSON.parse(text);
      certificateError.value = data.message;
    } else {
      certificateError.value = "Zertifikat konnte nicht heruntergeladen werden.";
    }
  }
};
</script>

<template>
  <div class="container mt-4">
    <h1 class="mb-4">Profile</h1>

    <div v-if="loading" class="alert alert-info">
      Profil wird geladen...
    </div>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-if="certificateError" class="alert alert-warning">
      {{ certificateError }}
    </div>

    <div class="card shadow-sm mb-4">
      <div class="card-body">
        <h4>{{ user?.fullName }}</h4>

        <p class="mb-1">
          <strong>E-Mail:</strong> {{ user?.email }}
        </p>

        <p class="mb-1">
          <strong>Rolle:</strong> {{ user?.role }}
        </p>
      </div>
    </div>

    <div class="row">
      <!-- Bewerbungen -->
      <div class="col-md-6 mb-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5>Meine Bewerbungen</h5>

            <p class="display-6">
              {{ applications.length }}
            </p>

            <ul v-if="applications.length > 0" class="list-group">
              <li
                v-for="application in applications"
                :key="application.id"
                class="list-group-item"
              >
                <strong>{{ application.job?.title }}</strong><br />
                Firma: {{ application.job?.company }}<br />
                Status: {{ application.status }}
              </li>
            </ul>

            <p v-else class="text-muted">
              Noch keine Bewerbungen vorhanden.
            </p>
          </div>
        </div>
      </div>

      <!-- Kurse -->
      <div class="col-md-6 mb-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5>Meine Kurse</h5>

            <p class="display-6">
              {{ enrollments.length }}
            </p>

            <ul v-if="enrollments.length > 0" class="list-group">
              <li
                v-for="enrollment in enrollments"
                :key="enrollment.id"
                class="list-group-item"
              >
                <strong>{{ enrollment.course?.title }}</strong><br />
                Level: {{ enrollment.course?.level }}<br />
                Kategorie: {{ enrollment.course?.category }}

                <button
                  class="btn btn-outline-success btn-sm mt-2 d-block"
                  @click="downloadCertificate(
                    enrollment.courseId,
                    enrollment.course?.title
                  )"
                >
                  Zertifikat herunterladen
                </button>
              </li>
            </ul>

            <p v-else class="text-muted">
              Du bist noch in keinem Kurs eingeschrieben.
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Progress Details -->
    <div class="card shadow-sm mt-4">
      <div class="card-body">
        <h5>Abgeschlossene Lektionen</h5>

        <ul class="list-group">
          <li
            v-for="item in progress"
            :key="item.id"
            class="list-group-item"
          >
            Lesson ID: {{ item.lessonId }} -
            abgeschlossen am {{ new Date(item.completedAt).toLocaleString() }}
          </li>
        </ul>

        <p v-if="progress.length === 0" class="text-muted mt-3">
          Noch keine Lektionen abgeschlossen.
        </p>
      </div>
    </div>
  </div>
</template>