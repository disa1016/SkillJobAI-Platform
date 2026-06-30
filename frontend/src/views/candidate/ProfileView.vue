<script setup>
import { computed, onMounted, ref } from "vue";

import {
  deleteCv,
  downloadCourseCertificate,
  getMyApplications,
  getMyEnrollments,
  getMyProgress,
  getProfile,
  uploadCv,
} from "@/services/candidateService";

import api from "@/services/api";
import { formatDate } from "@/utils/date";

const MAX_CV_SIZE = 5 * 1024 * 1024;

const user = ref(JSON.parse(localStorage.getItem("user")) || null);

const applications = ref([]);
const enrollments = ref([]);
const progress = ref([]);

const selectedCv = ref(null);

const loading = ref(true);
const error = ref("");
const cvMessage = ref("");
const cvError = ref("");
const certificateError = ref("");

const backendUrl = computed(() => {
  const baseUrl = api.defaults.baseURL || "";
  return baseUrl.replace("/api", "");
});

const cvFullUrl = computed(() => {
  const cvUrl = user.value?.cvUrl;

  if (!cvUrl) return "";
  if (cvUrl.startsWith("http")) return cvUrl;

  return `${backendUrl.value}${cvUrl}`;
});

const hasApplications = computed(() => applications.value.length > 0);
const hasEnrollments = computed(() => enrollments.value.length > 0);
const hasProgress = computed(() => progress.value.length > 0);

const formatDateTime = (date) => {
  if (!date) return "Kein Datum";

  return new Date(date).toLocaleString("de-DE");
};

const clearCvMessages = () => {
  cvMessage.value = "";
  cvError.value = "";
};

const loadProfile = async () => {
  const data = await getProfile();

  user.value = data;
  localStorage.setItem("user", JSON.stringify(data));
};

const loadProfileData = async () => {
  loading.value = true;
  error.value = "";

  try {
    const [applicationsData, enrollmentsData, progressData] = await Promise.all([
      getMyApplications(),
      getMyEnrollments(),
      getMyProgress(),
      loadProfile(),
    ]);

    applications.value = applicationsData;
    enrollments.value = enrollmentsData;
    progress.value = progressData;
  } catch {
    error.value = "Profil-Daten konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};

const handleCvChange = (event) => {
  clearCvMessages();

  const file = event.target.files?.[0];

  if (!file) {
    selectedCv.value = null;
    return;
  }

  if (file.type !== "application/pdf") {
    selectedCv.value = null;
    cvError.value = "Bitte nur PDF-Dateien hochladen.";
    event.target.value = "";
    return;
  }

  if (file.size > MAX_CV_SIZE) {
    selectedCv.value = null;
    cvError.value = "Die PDF-Datei darf maximal 5MB groß sein.";
    event.target.value = "";
    return;
  }

  selectedCv.value = file;
};

const handleUploadCv = async () => {
  clearCvMessages();

  if (!selectedCv.value) {
    cvError.value = "Bitte zuerst eine PDF-Datei auswählen.";
    return;
  }

  try {
    await uploadCv(selectedCv.value);
    await loadProfile();

    selectedCv.value = null;
    cvMessage.value = "Lebenslauf wurde erfolgreich hochgeladen.";
  } catch (err) {
    cvError.value =
      err.response?.data?.message || "Lebenslauf konnte nicht hochgeladen werden.";
  }
};

const handleDeleteCv = async () => {
  clearCvMessages();

  try {
    await deleteCv();
    await loadProfile();

    selectedCv.value = null;
    cvMessage.value = "Lebenslauf wurde gelöscht.";
  } catch (err) {
    cvError.value =
      err.response?.data?.message || "Lebenslauf konnte nicht gelöscht werden.";
  }
};

const downloadCertificate = async (courseId, courseTitle) => {
  certificateError.value = "";

  try {
    const blobData = await downloadCourseCertificate(courseId);

    const blob = new Blob([blobData], {
      type: "application/pdf",
    });

    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = `certificate-${courseTitle || courseId}.pdf`;
    link.click();

    window.URL.revokeObjectURL(url);
  } catch (err) {
    if (err.response?.data instanceof Blob) {
      const text = await err.response.data.text();
      const data = JSON.parse(text);

      certificateError.value = data.message;
      return;
    }

    certificateError.value = "Zertifikat konnte nicht heruntergeladen werden.";
  }
};

onMounted(loadProfileData);
</script>

<template>
  <div class="container mt-4">
    <h1 class="mb-4">Profile</h1>

    <div v-if="loading" class="alert alert-info">
      Profil wird geladen...
    </div>

    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <template v-else>
      <div v-if="certificateError" class="alert alert-warning">
        {{ certificateError }}
      </div>

      <div class="card shadow-sm mb-4">
        <div class="card-body">
          <h4>{{ user?.fullName || "Unbekannter User" }}</h4>

          <p class="mb-1">
            <strong>E-Mail:</strong>
            {{ user?.email || "Keine E-Mail" }}
          </p>

          <p class="mb-1">
            <strong>Rolle:</strong>
            {{ user?.role || "Keine Rolle" }}
          </p>

          <p v-if="user?.createdAt" class="mb-0">
            <strong>Mitglied seit:</strong>
            {{ formatDate(user.createdAt) }}
          </p>
        </div>
      </div>

      <div class="card shadow-sm mb-4">
        <div class="card-body">
          <h5 class="mb-3">Lebenslauf / CV</h5>

          <div v-if="cvMessage" class="alert alert-success">
            {{ cvMessage }}
          </div>

          <div v-if="cvError" class="alert alert-danger">
            {{ cvError }}
          </div>

          <div v-if="user?.cvUrl" class="mb-3">
            <p class="mb-2">
              <strong>Aktueller Lebenslauf:</strong>
            </p>

            <a :href="cvFullUrl" target="_blank" rel="noopener noreferrer" class="btn btn-outline-primary btn-sm me-2">
              CV anzeigen
            </a>

            <button type="button" class="btn btn-outline-danger btn-sm" @click="handleDeleteCv">
              CV löschen
            </button>
          </div>

          <div v-else class="alert alert-light border">
            Du hast noch keinen Lebenslauf hochgeladen.
          </div>

          <div class="mt-3">
            <label class="form-label">
              PDF-Lebenslauf hochladen
            </label>

            <input type="file" accept="application/pdf" class="form-control" @change="handleCvChange" />

            <small class="text-muted">
              Erlaubt: PDF, maximal 5MB.
            </small>

            <div class="mt-3">
              <button type="button" class="btn btn-primary" :disabled="!selectedCv" @click="handleUploadCv">
                Lebenslauf hochladen
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="row">
        <div class="col-md-6 mb-3">
          <div class="card shadow-sm h-100">
            <div class="card-body">
              <h5>Meine Bewerbungen</h5>

              <p class="display-6">
                {{ applications.length }}
              </p>

              <ul v-if="hasApplications" class="list-group">
                <li v-for="application in applications" :key="application.id" class="list-group-item">
                  <strong>{{ application.job?.title || "Job gelöscht" }}</strong>
                  <br />
                  Firma: {{ application.job?.company || "Keine Firma" }}
                  <br />
                  Status: {{ application.status || "Unbekannt" }}
                </li>
              </ul>

              <p v-else class="text-muted">
                Noch keine Bewerbungen vorhanden.
              </p>
            </div>
          </div>
        </div>

        <div class="col-md-6 mb-3">
          <div class="card shadow-sm h-100">
            <div class="card-body">
              <h5>Meine Kurse</h5>

              <p class="display-6">
                {{ enrollments.length }}
              </p>

              <ul v-if="hasEnrollments" class="list-group">
                <li v-for="enrollment in enrollments" :key="enrollment.id" class="list-group-item">
                  <strong>{{ enrollment.course?.title || "Kurs gelöscht" }}</strong>
                  <br />
                  Level: {{ enrollment.course?.level || "Kein Level" }}
                  <br />
                  Kategorie: {{ enrollment.course?.category || "Keine Kategorie" }}

                  <button type="button" class="btn btn-outline-success btn-sm mt-2 d-block" @click="
                    downloadCertificate(
                      enrollment.courseId,
                      enrollment.course?.title
                    )
                    ">
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

      <div class="card shadow-sm mt-4">
        <div class="card-body">
          <h5>Abgeschlossene Lektionen</h5>

          <ul v-if="hasProgress" class="list-group">
            <li v-for="item in progress" :key="item.id" class="list-group-item">
              Lesson ID: {{ item.lessonId }} -
              abgeschlossen am {{ formatDateTime(item.completedAt) }}
            </li>
          </ul>

          <p v-else class="text-muted mt-3">
            Noch keine Lektionen abgeschlossen.
          </p>
        </div>
      </div>
    </template>
  </div>
</template>