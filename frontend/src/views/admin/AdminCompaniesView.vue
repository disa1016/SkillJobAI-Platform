<script setup>
import { computed, onMounted, ref } from "vue";
import api from "@/services/api";
import BasePagination from "@/components/shared/BasePagination.vue";

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

const showCreateForm = ref(false);

const newCompany = ref({
    name: "",
    description: "",
    websiteUrl: "",
    location: "",
});

const hasCompanies = computed(() => companies.value.length > 0);
const canGoPrevious = computed(() => page.value > 1);
const canGoNext = computed(() => page.value < totalPages.value);

const backendUrl = computed(() => {
    const baseUrl = api.defaults.baseURL || "";
    return baseUrl.replace("/api", "");
});

const getLogoSrc = (logoUrl) => {
    if (!logoUrl) return "";

    if (logoUrl.startsWith("http")) {
        return logoUrl;
    }

    return `${backendUrl.value}${logoUrl}`;
};

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const resetCreateForm = () => {
    newCompany.value = {
        name: "",
        description: "",
        websiteUrl: "",
        location: "",
    };
};

const openCreateForm = () => {
    clearMessages();
    resetCreateForm();
    showCreateForm.value = true;
};

const closeCreateForm = () => {
    clearMessages();
    resetCreateForm();
    showCreateForm.value = false;
};

const normalizeUrl = (value) => {
    const trimmedValue = value?.trim();

    if (!trimmedValue) {
        return null;
    }

    if (/^https?:\/\//i.test(trimmedValue)) {
        return trimmedValue;
    }

    return `https://${trimmedValue}`;
};

const getApiErrorMessage = (
    err,
    fallbackMessage
) => {
    const validationErrors =
        err.response?.data?.errors;

    if (validationErrors) {
        return Object.values(validationErrors)
            .flat()
            .join(" ");
    }

    return (
        err.response?.data?.message ||
        err.response?.data?.title ||
        fallbackMessage
    );
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
    } catch (err) {
        console.error(
            "Firmen konnten nicht geladen werden:",
            err.response?.data || err
        );

        error.value = getApiErrorMessage(
            err,
            "Firmen konnten nicht geladen werden."
        );
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

const createCompany = async () => {
    clearMessages();

    const name = newCompany.value.name.trim();

    if (!name) {
        error.value =
            "Bitte einen Firmennamen eingeben.";
        return;
    }

    loading.value = true;

    try {
        await api.post("/companies", {
            name,
            description:
                newCompany.value.description.trim() ||
                null,
            websiteUrl: normalizeUrl(
                newCompany.value.websiteUrl
            ),
            location:
                newCompany.value.location.trim() ||
                null,
        });

        page.value = 1;
        search.value = "";

        resetCreateForm();
        showCreateForm.value = false;

        await loadCompanies();

        success.value =
            "Firma wurde erfolgreich erstellt.";
    } catch (err) {
        console.error(
            "Firma konnte nicht erstellt werden:",
            err.response?.data || err
        );

        error.value = getApiErrorMessage(
            err,
            "Firma konnte nicht erstellt werden."
        );
    } finally {
        loading.value = false;
    }
};

const updateCompany = async (company) => {
    clearMessages();

    const companyName =
        company.name?.trim();

    if (!companyName) {
        error.value =
            "Der Firmenname darf nicht leer sein.";
        return;
    }

    loading.value = true;

    try {
        await api.put(
            `/companies/${company.id}`,
            {
                name: companyName,
                description:
                    company.description?.trim() ||
                    null,
                websiteUrl: normalizeUrl(
                    company.websiteUrl
                ),
                logoUrl:
                    company.logoUrl?.trim() ||
                    null,
                location:
                    company.location?.trim() ||
                    null,
            }
        );

        await loadCompanies();

        success.value =
            "Firma wurde aktualisiert.";
    } catch (err) {
        console.error(
            "Firma konnte nicht aktualisiert werden:",
            err.response?.data || err
        );

        error.value = getApiErrorMessage(
            err,
            "Firma konnte nicht aktualisiert werden."
        );
    } finally {
        loading.value = false;
    }
};

const handleLogoSelected = (
    companyId,
    event
) => {
    const file =
        event.target.files?.[0];

    if (!file) {
        selectedFiles.value[companyId] =
            null;
        return;
    }

    selectedFiles.value[companyId] =
        file;
};

const uploadLogo = async (company) => {
    clearMessages();

    const file =
        selectedFiles.value[company.id];

    if (!file) {
        error.value =
            "Bitte zuerst eine Logo-Datei auswählen.";
        return;
    }

    loading.value = true;

    try {
        const formData =
            new FormData();

        formData.append("file", file);

        const { data } = await api.post(
            `/companies/${company.id}/logo`,
            formData,
            {
                headers: {
                    "Content-Type":
                        "multipart/form-data",
                },
            }
        );

        company.logoUrl = data.logoUrl;
        selectedFiles.value[company.id] =
            null;

        await loadCompanies();

        success.value =
            "Logo wurde erfolgreich hochgeladen.";
    } catch (err) {
        console.error(
            "Logo konnte nicht hochgeladen werden:",
            err.response?.data || err
        );

        error.value = getApiErrorMessage(
            err,
            "Logo konnte nicht hochgeladen werden."
        );
    } finally {
        loading.value = false;
    }
};

const deleteCompany = async (id) => {
    const confirmed = confirm(
        "Möchtest du diese Firma wirklich löschen?"
    );

    if (!confirmed) return;

    clearMessages();
    loading.value = true;

    try {
        await api.delete(
            `/companies/${id}`
        );

        if (
            companies.value.length === 1 &&
            page.value > 1
        ) {
            page.value -= 1;
        }

        await loadCompanies();

        success.value =
            "Firma wurde gelöscht.";
    } catch (err) {
        console.error(
            "Firma konnte nicht gelöscht werden:",
            err.response?.data || err
        );

        error.value = getApiErrorMessage(
            err,
            "Firma konnte nicht gelöscht werden."
        );
    } finally {
        loading.value = false;
    }
};

onMounted(loadCompanies);
</script>

<template>
    <div class="container py-4">
        <!-- Firmenübersicht -->
        <template v-if="!showCreateForm">
            <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-3">
                <h2 class="mb-0">
                    Firmen verwalten
                </h2>

                <div class="d-flex flex-wrap gap-2">
                    <button type="button" class="btn btn-success btn-sm" :disabled="loading" @click="openCreateForm">
                        Neue Firma
                    </button>

                    <button type="button" class="btn btn-outline-primary btn-sm" :disabled="loading"
                        @click="loadCompanies">
                        Aktualisieren
                    </button>
                </div>
            </div>

            <div class="d-flex flex-wrap gap-2 mb-3">
                <input v-model="search" type="text" class="form-control" style="max-width: 320px"
                    placeholder="Firma suchen..." @keyup.enter="
                        searchCompanies
                    " />

                <button type="button" class="btn btn-primary" :disabled="loading" @click="searchCompanies">
                    Suchen
                </button>

                <button type="button" class="btn btn-outline-secondary" :disabled="loading" @click="clearSearch">
                    Zurücksetzen
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

            <template v-if="!loading">
                <p class="text-muted">
                    {{ totalItems }}
                    Firmen gefunden · Seite
                    {{ page }} von
                    {{ totalPages }}
                </p>

                <div class="card shadow-sm">
                    <div class="card-body">
                        <div class="table-responsive">
                            <table class="table table-striped align-middle mb-0">
                                <thead>
                                    <tr>
                                        <th>
                                            ID
                                        </th>

                                        <th>
                                            Logo
                                        </th>

                                        <th>
                                            Name
                                        </th>

                                        <th>
                                            Standort
                                        </th>

                                        <th>
                                            Website
                                        </th>

                                        <th>
                                            Logo Upload
                                        </th>

                                        <th>
                                            Jobs
                                        </th>

                                        <th>
                                            Aktionen
                                        </th>
                                    </tr>
                                </thead>

                                <tbody>
                                    <tr v-for="company in companies" :key="company.id
                                        ">
                                        <td>
                                            {{
                                                company.id
                                            }}
                                        </td>

                                        <td>
                                            <img v-if="
                                                company.logoUrl
                                            " :src="getLogoSrc(
                                                    company.logoUrl
                                                )
                                                    " :alt="company.name
                                                    " style="
                                                    width: 48px;
                                                    height: 48px;
                                                    object-fit: contain;
                                                " class="border rounded bg-light p-1" />

                                            <span v-else class="text-muted small">
                                                Kein Logo
                                            </span>
                                        </td>

                                        <td style="
                                                min-width: 180px;
                                            ">
                                            <input v-model="company.name
                                                " class="form-control form-control-sm" maxlength="100" />
                                        </td>

                                        <td style="
                                                min-width: 160px;
                                            ">
                                            <input v-model="company.location
                                                " class="form-control form-control-sm" maxlength="100" />
                                        </td>

                                        <td style="
                                                min-width: 220px;
                                            ">
                                            <input v-model="company.websiteUrl
                                                " class="form-control form-control-sm"
                                                placeholder="https://example.com" />
                                        </td>

                                        <td style="
                                                min-width: 260px;
                                            ">
                                            <input type="file" accept=".jpg,.jpeg,.png,.webp"
                                                class="form-control form-control-sm mb-2" @change="
                                                    handleLogoSelected(
                                                        company.id,
                                                        $event
                                                    )
                                                    " />

                                            <button type="button" class="btn btn-outline-success btn-sm" :disabled="loading ||
                                                !selectedFiles[
                                                company
                                                    .id
                                                ]
                                                " @click="
                                                    uploadLogo(
                                                        company
                                                    )
                                                    ">
                                                Logo hochladen
                                            </button>
                                        </td>

                                        <td>
                                            {{
                                                company.totalJobs ??
                                                0
                                            }}
                                        </td>

                                        <td>
                                            <div class="d-flex flex-wrap gap-2">
                                                <button type="button" class="btn btn-primary btn-sm" :disabled="loading
                                                    " @click="
                                                        updateCompany(
                                                            company
                                                        )
                                                        ">
                                                    Speichern
                                                </button>

                                                <button type="button" class="btn btn-danger btn-sm" :disabled="loading
                                                    " @click="
                                                        deleteCompany(
                                                            company.id
                                                        )
                                                        ">
                                                    Löschen
                                                </button>
                                            </div>
                                        </td>
                                    </tr>

                                    <tr v-if="
                                        !hasCompanies
                                    ">
                                        <td colspan="8" class="text-center text-muted">
                                            Keine Firmen
                                            gefunden.
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <BasePagination :page="page" :total-pages="totalPages
                            " :can-go-previous="canGoPrevious
                                " :can-go-next="canGoNext
                                " @previous="
                                goToPreviousPage
                            " @next="
                                goToNextPage
                            " />
                    </div>
                </div>
            </template>
        </template>

        <!-- Formular zum Erstellen -->
        <template v-else>
            <div class="d-flex justify-content-between align-items-center gap-3 mb-3">
                <h2 class="mb-0">
                    Neue Firma erstellen
                </h2>

                <button type="button" class="btn btn-outline-secondary" :disabled="loading" @click="closeCreateForm">
                    Zurück zu Companies
                </button>
            </div>

            <div v-if="error" class="alert alert-danger">
                {{ error }}
            </div>

            <div class="card shadow-sm">
                <div class="card-body">
                    <form @submit.prevent="
                        createCompany
                    ">
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label for="companyName" class="form-label">
                                    Firmenname *
                                </label>

                                <input id="companyName" v-model="newCompany.name
                                    " type="text" class="form-control" maxlength="100" required autofocus
                                    placeholder="z. B. Musterfirma GmbH" />
                            </div>

                            <div class="col-md-6">
                                <label for="companyLocation" class="form-label">
                                    Standort
                                </label>

                                <input id="companyLocation" v-model="newCompany.location
                                    " type="text" class="form-control" maxlength="100" placeholder="z. B. Berlin" />
                            </div>

                            <div class="col-12">
                                <label for="companyWebsite" class="form-label">
                                    Website
                                </label>

                                <input id="companyWebsite" v-model="newCompany.websiteUrl
                                    " type="text" class="form-control" placeholder="z. B. example.com" />

                                <div class="form-text">
                                    https:// wird
                                    automatisch
                                    ergänzt.
                                </div>
                            </div>

                            <div class="col-12">
                                <label for="companyDescription" class="form-label">
                                    Beschreibung
                                </label>

                                <textarea id="companyDescription" v-model="newCompany.description
                                    " class="form-control" rows="5" maxlength="1000"
                                    placeholder="Kurze Beschreibung der Firma"></textarea>
                            </div>
                        </div>

                        <div class="d-flex flex-wrap gap-2 mt-4">
                            <button type="submit" class="btn btn-success" :disabled="loading
                                ">
                                <span v-if="
                                    loading
                                " class="spinner-border spinner-border-sm me-2"></span>

                                Firma erstellen
                            </button>

                            <button type="button" class="btn btn-outline-secondary" :disabled="loading
                                " @click="
                                    closeCreateForm
                                ">
                                Abbrechen
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </template>
    </div>
</template>