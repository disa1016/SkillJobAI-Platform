<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const companies = ref([]);
const loading = ref(false);
const error = ref("");
const success = ref("");

const selectedFiles = ref({});

const getLogoSrc = (logoUrl) => {
    if (!logoUrl) return "";

    if (logoUrl.startsWith("http")) {
        return logoUrl;
    }

    return `${api.defaults.baseURL}${logoUrl}`;
};

const loadCompanies = async () => {
    loading.value = true;
    error.value = "";

    try {
        const response = await api.get("/companies");
        companies.value = response.data;
    } catch {
        error.value = "Firmen konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const updateCompany = async (company) => {
    error.value = "";
    success.value = "";

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
    const file = event.target.files[0];

    if (!file) return;

    selectedFiles.value[companyId] = file;
};

const uploadLogo = async (company) => {
    error.value = "";
    success.value = "";

    const file = selectedFiles.value[company.id];

    if (!file) {
        error.value = "Bitte zuerst eine Logo-Datei auswählen.";
        return;
    }

    try {
        const formData = new FormData();
        formData.append("file", file);

        const response = await api.post(
            `/companies/${company.id}/logo`,
            formData,
            {
                headers: {
                    "Content-Type": "multipart/form-data",
                },
            }
        );

        company.logoUrl = response.data.logoUrl;
        selectedFiles.value[company.id] = null;

        success.value = "Logo wurde erfolgreich hochgeladen.";
    } catch (err) {
        error.value =
            err.response?.data?.message || "Logo konnte nicht hochgeladen werden.";
    }
};

const deleteCompany = async (id) => {
    if (!confirm("Möchtest du diese Firma wirklich löschen?")) return;

    error.value = "";
    success.value = "";

    try {
        await api.delete(`/companies/${id}`);
        companies.value = companies.value.filter((c) => c.id !== id);
        success.value = "Firma wurde gelöscht.";
    } catch {
        error.value = "Firma konnte nicht gelöscht werden.";
    }
};

const createCompany = async () => {
    error.value = "";
    success.value = "";

    try {
        const response = await api.post("/companies", {
            name: "Neue Firma",
            description: "",
            websiteUrl: "",
            logoUrl: "",
            location: "",
        });

        companies.value.push({
            ...response.data,
            totalJobs: 0,
        });

        success.value = "Neue Firma wurde erstellt.";
    } catch {
        error.value = "Firma konnte nicht erstellt werden.";
    }
};

onMounted(loadCompanies);
</script>

<template>
    <div class="container py-4">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h2 class="mb-0">Firmen verwalten</h2>

            <div>
                <button class="btn btn-success btn-sm me-2" @click="createCompany">
                    Neue Firma
                </button>

                <button class="btn btn-outline-primary btn-sm" @click="loadCompanies">
                    Aktualisieren
                </button>
            </div>
        </div>

        <div v-if="loading" class="alert alert-info">
            Firmen werden geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div v-if="!loading" class="card shadow-sm">
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
                            <tr v-for="company in companies" :key="company.id">
                                <td>{{ company.id }}</td>

                                <td>
                                    <img v-if="company.logoUrl" :src="getLogoSrc(company.logoUrl)" :alt="company.name"
                                        style="width: 48px; height: 48px; object-fit: contain;"
                                        class="border rounded bg-light p-1" />

                                    <span v-else class="text-muted small">
                                        Kein Logo
                                    </span>
                                </td>

                                <td style="min-width: 180px;">
                                    <input v-model="company.name" class="form-control form-control-sm" />
                                </td>

                                <td>
                                    <input v-model="company.location" class="form-control form-control-sm" />
                                </td>

                                <td style="min-width: 220px;">
                                    <input v-model="company.websiteUrl" class="form-control form-control-sm" />
                                </td>

                                <td style="min-width: 260px;">
                                    <input type="file" accept=".jpg,.jpeg,.png,.webp"
                                        class="form-control form-control-sm mb-2"
                                        @change="handleLogoSelected(company.id, $event)" />

                                    <button class="btn btn-outline-success btn-sm" @click="uploadLogo(company)">
                                        Logo hochladen
                                    </button>
                                </td>

                                <td>{{ company.totalJobs }}</td>

                                <td>
                                    <button class="btn btn-primary btn-sm me-2" @click="updateCompany(company)">
                                        Speichern
                                    </button>

                                    <button class="btn btn-danger btn-sm" @click="deleteCompany(company.id)">
                                        Löschen
                                    </button>
                                </td>
                            </tr>

                            <tr v-if="companies.length === 0">
                                <td colspan="8" class="text-center text-muted">
                                    Keine Firmen gefunden.
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</template>