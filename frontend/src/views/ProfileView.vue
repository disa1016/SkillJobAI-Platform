<script setup>
import { onMounted, ref, computed } from "vue";
import api from "../services/api";

const user = ref(JSON.parse(localStorage.getItem("user")));

const applications = ref([]);
const enrollments = ref([]);
const progress = ref([]);

const selectedCv = ref(null);
const cvMessage = ref("");
const cvError = ref("");

const loading = ref(true);
const error = ref("");
const certificateError = ref("");

const backendUrl = computed(() => {
  const baseUrl = api.defaults.baseURL || "";
  return baseUrl.replace("/api", "");
});

const cvFullUrl = computed(() => {
  if (!user.value?.cvUrl) return "";
  if (user.value.cvUrl.startsWith("http")) return user.value.cvUrl;
  return `${backendUrl.value}${user.value.cvUrl}`;
});

const loadProfile = async () => {
  const response = await api.get("/users/profile");
  user.value = response.data;
  localStorage.setItem("user", JSON.stringify(response.data));
};

onMounted(async () => {
  try {
    await loadProfile();

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

const handleCvChange = (event) => {
  cvMessage.value = "";
  cvError.value = "";

  const file = event.target.files[0];

  if (!file) {
    selectedCv.value = null;
    return;
  }

  if (file.type !== "application/pdf") {
    cvError.value = "Bitte nur PDF-Dateien hochladen.";
    selectedCv.value = null;
    return;
  }

  if (file.size > 5 * 1024 * 1024) {
    cvError.value = "Die PDF-Datei darf maximal 5MB groß sein.";
    selectedCv.value = null;
    return;
  }

  selectedCv.value = file;
};

const uploadCv = async () => {
  cvMessage.value = "";
  cvError.value = "";

  if (!selectedCv.value) {
    cvError.value = "Bitte zuerst eine PDF-Datei auswählen.";
    return;
  }

  try {
    const formData = new FormData();
    formData.append("file", selectedCv.value);

    await api.post("/users/cv", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    await loadProfile();

    selectedCv.value = null;
    cvMessage.value = "Lebenslauf wurde erfolgreich hochgeladen.";
  } catch (err) {
    cvError.value =
      err.response?.data?.message || "Lebenslauf konnte nicht hochgeladen werden.";
  }
};

const deleteCv = async () => {
  cvMessage.value = "";
  cvError.value = "";

  try {
    await api.delete("/users/cv");
    await loadProfile();

    cvMessage.value = "Lebenslauf wurde gelöscht.";
  } catch (err) {
    cvError.value =
      err.response?.data?.message || "Lebenslauf konnte nicht gelöscht werden.";
  }
};

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

    <div v-if="!loading" class="card shadow-sm mb-4">
      <div class="card-body">
        <h4>{{ user?.fullName }}</h4>

        <p class="mb-1">
          <strong>E-Mail:</strong> {{ user?.email }}
        </p>

        <p class="mb-1">
          <strong>Rolle:</strong> {{ user?.role }}
        </p>

        <p class="mb-0" v-if="user?.createdAt">
          <strong>Mitglied seit:</strong>
          {{ new Date(user.createdAt).toLocaleDateString("de-DE") }}
        </p>
      </div>
    </div>

    <div v-if="!loading" class="card shadow-sm mb-4">
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

          <button class="btn btn-outline-danger btn-sm" @click="deleteCv">
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
            <button class="btn btn-primary" @click="uploadCv" :disabled="!selectedCv">
              Lebenslauf hochladen
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="!loading" class="row">
      <div class="col-md-6 mb-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5>Meine Bewerbungen</h5>

            <p class="display-6">
              {{ applications.length }}
            </p>

            <ul v-if="applications.length > 0" class="list-group">
              <li v-for="application in applications" :key="application.id" class="list-group-item">
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

      <div class="col-md-6 mb-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5>Meine Kurse</h5>

            <p class="display-6">
              {{ enrollments.length }}
            </p>

            <ul v-if="enrollments.length > 0" class="list-group">
              <li v-for="enrollment in enrollments" :key="enrollment.id" class="list-group-item">
                <strong>{{ enrollment.course?.title }}</strong><br />
                Level: {{ enrollment.course?.level }}<br />
                Kategorie: {{ enrollment.course?.category }}

                <button class="btn btn-outline-success btn-sm mt-2 d-block" @click="downloadCertificate(
                  enrollment.courseId,
                  enrollment.course?.title
                )">
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

    <div v-if="!loading" class="card shadow-sm mt-4">
      <div class="card-body">
        <h5>Abgeschlossene Lektionen</h5>

        <ul class="list-group">
          <li v-for="item in progress" :key="item.id" class="list-group-item">
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