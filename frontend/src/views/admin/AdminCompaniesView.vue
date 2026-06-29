<script setup>
import { computed, onMounted, ref } from "vue";
import api from "@/services/api";

const companies = ref([]);
const selectedFiles = ref({});

const loading = ref(false);
const error = ref("");
const success = ref("");

const page = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);
const totalItems = ref(0);
const search = ref("");

const hasCompanies = computed(() => companies.value.length > 0);
const canGoPrevious = computed(() => page.value > 1);
const canGoNext = computed(() => page.value < totalPages.value);

const backendUrl = computed(() => {
  const baseUrl = api.defaults.baseURL || "";
  return baseUrl.replace("/api", "");
});

const getLogoSrc = (logoUrl) => {
  if (!logoUrl) return "";
  if (logoUrl.startsWith("http")) return logoUrl;

  return `${backendUrl.value}${logoUrl}`;
};

const clearMessages = () => {
  error.value = "";
  success.value = "";
};

const loadCompanies = async () => {
  loading.value = true;
  clearMessages();

  try {
    const { data } = await api.get("/companies", {
      params: {
        page: page.value,
        pageSize: pageSize.value,
        search: search.value,
      },
    });

    companies.value = data.items;
    totalPages.value = data.totalPages;
    totalItems.value = data.totalItems;
  } catch {
    error.value = "Firmen konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};

const searchCompanies = async () => {
  page.value = 1;
  await loadCompanies();
};

const clearSearch = async () => {
  search.value = "";
  page.value = 1;
  await loadCompanies();
};

const goToPreviousPage = async () => {
  if (!canGoPrevious.value) return;

  page.value -= 1;
  await loadCompanies();
};

const goToNextPage = async () => {
  if (!canGoNext.value) return;

  page.value += 1;
  await loadCompanies();
};

const updateCompany = async (company) => {
  clearMessages();

  try {
    await api.put(`/companies/${company.id}`, {
      name: company.name,
      description: company.description,
      websiteUrl: company.websiteUrl,
      logoUrl: company.logoUrl,
      location: company.location,
    });

    success.value = "Firma wurde aktualisiert.";
  } catch {
    error.value = "Firma konnte nicht aktualisiert werden.";
  }
};

const handleLogoSelected = (companyId, event) => {
  const file = event.target.files?.[0];

  if (!file) {
    selectedFiles.value[companyId] = null;
    return;
  }

  selectedFiles.value[companyId] = file;
};

const uploadLogo = async (company) => {
  clearMessages();

  const file = selectedFiles.value[company.id];

  if (!file) {
    error.value = "Bitte zuerst eine Logo-Datei auswählen.";
    return;
  }

  try {
    const formData = new FormData();
    formData.append("file", file);

    const { data } = await api.post(`/companies/${company.id}/logo`, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    company.logoUrl = data.logoUrl;
    selectedFiles.value[company.id] = null;

    success.value = "Logo wurde erfolgreich hochgeladen.";
  } catch (err) {
    error.value =
      err.response?.data?.message || "Logo konnte nicht hochgeladen werden.";
  }
};

const deleteCompany = async (id) => {
  if (!confirm("Möchtest du diese Firma wirklich löschen?")) return;

  clearMessages();

  try {
    await api.delete(`/companies/${id}`);

    if (companies.value.length === 1 && page.value > 1) {
      page.value -= 1;
    }

    success.value = "Firma wurde gelöscht.";
    await loadCompanies();
  } catch {
    error.value = "Firma konnte nicht gelöscht werden.";
  }
};

const createCompany = async () => {
  clearMessages();

  try {
    await api.post("/companies", {
      name: "Neue Firma",
      description: "",
      websiteUrl: "",
      logoUrl: "",
      location: "",
    });

    page.value = 1;
    search.value = "";
    success.value = "Neue Firma wurde erstellt.";

    await loadCompanies();
  } catch {
    error.value = "Firma konnte nicht erstellt werden.";
  }
};

onMounted(loadCompanies);
</script>

<template>
  <div class="container py-4">
    <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-3">
      <h2 class="mb-0">Firmen verwalten</h2>

      <div class="d-flex flex-wrap gap-2">
        <button
          type="button"
          class="btn btn-success btn-sm"
          :disabled="loading"
          @click="createCompany"
        >
          Neue Firma
        </button>

        <button
          type="button"
          class="btn btn-outline-primary btn-sm"
          :disabled="loading"
          @click="loadCompanies"
        >
          Aktualisieren
        </button>
      </div>
    </div>

    <div class="d-flex flex-wrap gap-2 mb-3">
      <input
        v-model="search"
        type="text"
        class="form-control"
        style="max-width: 320px"
        placeholder="Firma suchen..."
        @keyup.enter="searchCompanies"
      />

      <button
        type="button"
        class="btn btn-primary"
        @click="searchCompanies"
      >
        Suchen
      </button>

      <button
        type="button"
        class="btn btn-outline-secondary"
        @click="clearSearch"
      >
        Zurücksetzen
      </button>
    </div>

    <div
      v-if="loading"
      class="alert alert-info"
    >
      Firmen werden geladen...
    </div>

    <div
      v-else-if="error"
      class="alert alert-danger"
    >
      {{ error }}
    </div>

    <template v-else>
      <div
        v-if="success"
        class="alert alert-success"
      >
        {{ success }}
      </div>

      <p class="text-muted">
        {{ totalItems }} Firmen gefunden · Seite {{ page }} von {{ totalPages }}
      </p>

      <div class="card shadow-sm">
        <div class="card-body">
          <div class="table-responsive">
            <table class="table table-striped align-middle mb-0">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Logo</th>
                  <th>Name</th>
                  <th>Standort</th>
                  <th>Website</th>
                  <th>Logo Upload</th>
                  <th>Jobs</th>
                  <th>Aktionen</th>
                </tr>
              </thead>

              <tbody>
                <tr
                  v-for="company in companies"
                  :key="company.id"
                >
                  <td>{{ company.id }}</td>

                  <td>
                    <img
                      v-if="company.logoUrl"
                      :src="getLogoSrc(company.logoUrl)"
                      :alt="company.name"
                      style="width: 48px; height: 48px; object-fit: contain"
                      class="border rounded bg-light p-1"
                    />

                    <span
                      v-else
                      class="text-muted small"
                    >
                      Kein Logo
                    </span>
                  </td>

                  <td style="min-width: 180px">
                    <input
                      v-model="company.name"
                      class="form-control form-control-sm"
                    />
                  </td>

                  <td>
                    <input
                      v-model="company.location"
                      class="form-control form-control-sm"
                    />
                  </td>

                  <td style="min-width: 220px">
                    <input
                      v-model="company.websiteUrl"
                      class="form-control form-control-sm"
                    />
                  </td>

                  <td style="min-width: 260px">
                    <input
                      type="file"
                      accept=".jpg,.jpeg,.png,.webp"
                      class="form-control form-control-sm mb-2"
                      @change="handleLogoSelected(company.id, $event)"
                    />

                    <button
                      type="button"
                      class="btn btn-outline-success btn-sm"
                      :disabled="!selectedFiles[company.id]"
                      @click="uploadLogo(company)"
                    >
                      Logo hochladen
                    </button>
                  </td>

                  <td>{{ company.totalJobs ?? 0 }}</td>

                  <td>
                    <div class="d-flex flex-wrap gap-2">
                      <button
                        type="button"
                        class="btn btn-primary btn-sm"
                        @click="updateCompany(company)"
                      >
                        Speichern
                      </button>

                      <button
                        type="button"
                        class="btn btn-danger btn-sm"
                        @click="deleteCompany(company.id)"
                      >
                        Löschen
                      </button>
                    </div>
                  </td>
                </tr>

                <tr v-if="!hasCompanies">
                  <td
                    colspan="8"
                    class="text-center text-muted"
                  >
                    Keine Firmen gefunden.
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="d-flex justify-content-center align-items-center gap-2 mt-4">
            <button
              type="button"
              class="btn btn-outline-primary"
              :disabled="!canGoPrevious"
              @click="goToPreviousPage"
            >
              Zurück
            </button>

            <span class="text-muted">
              Seite {{ page }} / {{ totalPages }}
            </span>

            <button
              type="button"
              class="btn btn-outline-primary"
              :disabled="!canGoNext"
              @click="goToNextPage"
            >
              Weiter
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>