<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const companies = ref([]);
const loading = ref(false);
const error = ref("");
const success = ref("");

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
        <div>
            <button class="btn btn-success btn-sm me-2" @click="createCompany">
                Neue Firma
            </button>

            <button class="btn btn-outline-primary btn-sm" @click="loadCompanies">
                Aktualisieren
            </button>
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
                <table class="table table-striped align-middle mb-0">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Name</th>
                            <th>Standort</th>
                            <th>Website</th>
                            <th>Jobs</th>
                            <th>Aktionen</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="company in companies" :key="company.id">
                            <td>{{ company.id }}</td>

                            <td style="min-width: 180px;">
                                <input v-model="company.name" class="form-control form-control-sm" />
                            </td>

                            <td>
                                <input v-model="company.location" class="form-control form-control-sm" />
                            </td>

                            <td>
                                <input v-model="company.websiteUrl" class="form-control form-control-sm" />
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
                            <td colspan="6" class="text-center text-muted">
                                Keine Firmen gefunden.
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>