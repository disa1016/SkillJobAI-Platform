<script setup>
import { onMounted, ref, computed } from "vue";
import { useRoute } from "vue-router";
import api from "../services/api";

const route = useRoute();

const candidate = ref(null);
const loading = ref(true);
const error = ref("");

const backendUrl = computed(() => {
  const baseUrl = api.defaults.baseURL || "";
  return baseUrl.replace("/api", "");
});

const getFileUrl = (fileUrl) => {
  if (!fileUrl) return "";
  if (fileUrl.startsWith("http")) return fileUrl;
  return `${backendUrl.value}${fileUrl}`;
};

const getStatusBadgeClass = (status) => {
  if (status === "Accepted") return "bg-success";
  if (status === "Rejected") return "bg-danger";
  if (status === "Reviewed") return "bg-info text-dark";
  return "bg-warning text-dark";
};

const formatDate = (date) => {
  return new Date(date).toLocaleDateString("de-DE");
};

onMounted(async () => {
  try {
    const response = await api.get(`/recruiter/candidates/${route.params.id}`);
    candidate.value = response.data;
  } catch {
    error.value = "Kandidatendetails konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <div class="container py-4">
    <router-link to="/recruiter/candidates" class="btn btn-outline-secondary mb-3">
      Zurück zu Kandidaten
    </router-link>

    <div v-if="loading" class="alert alert-info">
      Kandidat wird geladen...
    </div>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-if="candidate">
      <div class="card shadow-sm mb-4">
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-start">
            <div>
              <h2 class="mb-1">
                {{ candidate.fullName }}
              </h2>

              <p class="text-muted mb-1">
                {{ candidate.email }}
              </p>

              <p class="text-muted mb-0">
                Registriert am {{ formatDate(candidate.createdAt) }}
              </p>
            </div>

            <span class="badge bg-primary fs-6">
              {{ candidate.skillsCount }} Skills
            </span>
          </div>

          <div class="mt-3">
            <a
              v-if="candidate.cvUrl"
              :href="getFileUrl(candidate.cvUrl)"
              target="_blank"
              rel="noopener noreferrer"
              class="btn btn-outline-primary btn-sm"
            >
              Profil-CV öffnen
            </a>

            <span v-else class="text-muted">
              Kein Profil-CV hochgeladen.
            </span>
          </div>
        </div>
      </div>

      <div class="row g-3 mb-4">
        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Bewerbungen</h6>
              <h3>{{ candidate.applicationsCount }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Accepted</h6>
              <h3 class="text-success">{{ candidate.acceptedApplications }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Rejected</h6>
              <h3 class="text-danger">{{ candidate.rejectedApplications }}</h3>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card shadow-sm border-0">
            <div class="card-body">
              <h6 class="text-muted">Skills</h6>
              <h3>{{ candidate.skillsCount }}</h3>
            </div>
          </div>
        </div>
      </div>

      <div class="card shadow-sm mb-4">
        <div class="card-body">
          <h5 class="mb-3">Skills</h5>

          <div v-if="candidate.skills?.length">
            <span
              v-for="skill in candidate.skills"
              :key="skill"
              class="badge bg-success me-2 mb-2"
            >
              {{ skill }}
            </span>
          </div>

          <p v-else class="text-muted mb-0">
            Keine Skills hinterlegt.
          </p>
        </div>
      </div>

      <div class="card shadow-sm">
        <div class="card-body">
          <h5 class="mb-3">Bewerbungen</h5>

          <div v-if="candidate.applications?.length === 0" class="text-muted">
            Keine Bewerbungen vorhanden.
          </div>

          <div
            v-for="application in candidate.applications"
            :key="application.id"
            class="border rounded p-3 mb-3"
          >
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <h6 class="mb-1">
                  {{ application.job?.title || "Job gelöscht" }}
                </h6>

                <p class="text-muted mb-1">
                  {{ application.job?.company || "Keine Firma" }}
                  · {{ formatDate(application.createdAt) }}
                </p>

                <p class="mb-1">
                  <strong>Standort:</strong>
                  {{ application.job?.location || "Keine Angabe" }}
                </p>

                <p class="mb-1">
                  <strong>Gehalt:</strong>
                  {{ application.job?.salary || "Keine Angabe" }}
                </p>
              </div>

              <span
                class="badge"
                :class="getStatusBadgeClass(application.status)"
              >
                {{ application.status }}
              </span>
            </div>

            <div class="mt-3">
              <strong>Bewerbungsunterlagen:</strong>

              <div class="d-flex flex-wrap gap-2 mt-2">
                <a
                  v-if="application.cvFileUrl"
                  :href="getFileUrl(application.cvFileUrl)"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="btn btn-sm btn-outline-primary"
                >
                  CV öffnen
                </a>

                <a
                  v-if="application.certificateFileUrl"
                  :href="getFileUrl(application.certificateFileUrl)"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="btn btn-sm btn-outline-secondary"
                >
                  Zeugnis öffnen
                </a>

                <a
                  v-if="application.portfolioFileUrl"
                  :href="getFileUrl(application.portfolioFileUrl)"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="btn btn-sm btn-outline-dark"
                >
                  Portfolio öffnen
                </a>

                <span
                  v-if="
                    !application.cvFileUrl &&
                    !application.certificateFileUrl &&
                    !application.portfolioFileUrl
                  "
                  class="text-muted"
                >
                  Keine Dateien hochgeladen.
                </span>
              </div>
            </div>

            <div v-if="application.coverLetter" class="mt-3">
              <strong>Anschreiben:</strong>
              <div class="border rounded bg-light p-3 mt-2">
                {{ application.coverLetter }}
              </div>
            </div>

            <router-link
              :to="`/recruiter/applications/${application.id}`"
              class="btn btn-sm btn-outline-primary mt-3"
            >
              Bewerbung Details ansehen
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>