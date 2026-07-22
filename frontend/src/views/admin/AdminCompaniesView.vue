<script setup>
import {
    computed,
    onBeforeUnmount,
    onMounted,
    ref,
} from "vue";

import api from "@/services/api";
import BasePagination from "@/components/shared/BasePagination.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

/*
|--------------------------------------------------------------------------
| Firmenübersicht
|--------------------------------------------------------------------------
*/

const companies = ref([]);
const selectedFiles = ref({});

const loading = ref(false);
const creating = ref(false);
const updatingCompanyId = ref(null);
const uploadingLogoCompanyId = ref(null);
const deletingCompanyId = ref(null);

const error = ref("");
const success = ref("");

const page = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);
const totalItems = ref(0);
const search = ref("");

/*
|--------------------------------------------------------------------------
| Formular zum Erstellen
|--------------------------------------------------------------------------
*/

const showCreateForm = ref(false);

const newCompany = ref({
    name: "",
    description: "",
    websiteUrl: "",
    location: "",
});

const createFormTouched = ref({
    name: false,
    description: false,
    websiteUrl: false,
    location: false,
    logo: false,
});

const newCompanyLogo = ref(null);
const newCompanyLogoPreview = ref("");
const newCompanyLogoError = ref("");

/*
|--------------------------------------------------------------------------
| Löschdialog
|--------------------------------------------------------------------------
*/

const showDeleteDialog = ref(false);
const companyToDelete = ref(null);

/*
|--------------------------------------------------------------------------
| Computed Properties
|--------------------------------------------------------------------------
*/

const hasCompanies = computed(
    () => companies.value.length > 0
);

const canGoPrevious = computed(
    () => page.value > 1
);

const canGoNext = computed(
    () => page.value < totalPages.value
);

const backendUrl = computed(() => {
    const baseUrl = api.defaults.baseURL || "";
    return baseUrl.replace(/\/api\/?$/, "");
});

const createFormErrors = computed(() => {
    const errors = {};

    const name = newCompany.value.name.trim();
    const location = newCompany.value.location.trim();
    const description =
        newCompany.value.description.trim();
    const website =
        newCompany.value.websiteUrl.trim();

    if (!name) {
        errors.name =
            "Bitte einen Firmennamen eingeben.";
    } else if (name.length > 100) {
        errors.name =
            "Der Firmenname darf höchstens 100 Zeichen enthalten.";
    }

    if (location.length > 100) {
        errors.location =
            "Der Standort darf höchstens 100 Zeichen enthalten.";
    }

    if (description.length > 1000) {
        errors.description =
            "Die Beschreibung darf höchstens 1000 Zeichen enthalten.";
    }

    if (website) {
        const normalizedWebsite =
            normalizeUrl(website);

        try {
            const parsedUrl =
                new URL(normalizedWebsite);

            if (
                parsedUrl.protocol !== "http:" &&
                parsedUrl.protocol !== "https:"
            ) {
                errors.websiteUrl =
                    "Bitte eine gültige Website eingeben.";
            }

            if (!parsedUrl.hostname.includes(".")) {
                errors.websiteUrl =
                    "Bitte eine vollständige Website eingeben, zum Beispiel example.com.";
            }
        } catch {
            errors.websiteUrl =
                "Bitte eine gültige Website eingeben.";
        }
    }

    if (newCompanyLogoError.value) {
        errors.logo =
            newCompanyLogoError.value;
    }

    return errors;
});

const isCreateFormValid = computed(
    () =>
        Object.keys(createFormErrors.value)
            .length === 0
);

/*
|--------------------------------------------------------------------------
| Hilfsfunktionen
|--------------------------------------------------------------------------
*/

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const getLogoSrc = (logoUrl) => {
    if (!logoUrl) {
        return "";
    }

    if (
        logoUrl.startsWith("http://") ||
        logoUrl.startsWith("https://") ||
        logoUrl.startsWith("blob:")
    ) {
        return logoUrl;
    }

    return `${backendUrl.value}${logoUrl}`;
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

const markCreateFieldTouched = (field) => {
    createFormTouched.value[field] = true;
};

const markAllCreateFieldsTouched = () => {
    Object.keys(
        createFormTouched.value
    ).forEach((field) => {
        createFormTouched.value[field] = true;
    });
};

const revokeLogoPreview = () => {
    if (
        newCompanyLogoPreview.value?.startsWith(
            "blob:"
        )
    ) {
        URL.revokeObjectURL(
            newCompanyLogoPreview.value
        );
    }

    newCompanyLogoPreview.value = "";
};

const resetCreateForm = () => {
    revokeLogoPreview();

    newCompany.value = {
        name: "",
        description: "",
        websiteUrl: "",
        location: "",
    };

    createFormTouched.value = {
        name: false,
        description: false,
        websiteUrl: false,
        location: false,
        logo: false,
    };

    newCompanyLogo.value = null;
    newCompanyLogoError.value = "";
};

const openCreateForm = () => {
    clearMessages();
    resetCreateForm();
    showCreateForm.value = true;
};

const closeCreateForm = () => {
    if (creating.value) {
        return;
    }

    clearMessages();
    resetCreateForm();
    showCreateForm.value = false;
};

const handleCreateWebsiteBlur = () => {
    markCreateFieldTouched("websiteUrl");

    const website =
        newCompany.value.websiteUrl.trim();

    if (!website) {
        return;
    }

    newCompany.value.websiteUrl =
        normalizeUrl(website);
};

/*
|--------------------------------------------------------------------------
| Logo beim Erstellen
|--------------------------------------------------------------------------
*/

const handleNewCompanyLogoSelected = (
    event
) => {
    markCreateFieldTouched("logo");

    revokeLogoPreview();

    const file =
        event.target.files?.[0];

    newCompanyLogo.value = null;
    newCompanyLogoError.value = "";

    if (!file) {
        return;
    }

    const allowedTypes = [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    const maximumFileSize =
        5 * 1024 * 1024;

    if (!allowedTypes.includes(file.type)) {
        newCompanyLogoError.value =
            "Bitte eine JPG-, PNG- oder WebP-Datei auswählen.";

        event.target.value = "";
        return;
    }

    if (file.size > maximumFileSize) {
        newCompanyLogoError.value =
            "Das Logo darf höchstens 5 MB groß sein.";

        event.target.value = "";
        return;
    }

    newCompanyLogo.value = file;
    newCompanyLogoPreview.value =
        URL.createObjectURL(file);
};

const removeNewCompanyLogo = () => {
    revokeLogoPreview();

    newCompanyLogo.value = null;
    newCompanyLogoError.value = "";
    createFormTouched.value.logo = false;

    const input =
        document.getElementById(
            "newCompanyLogo"
        );

    if (input) {
        input.value = "";
    }
};

/*
|--------------------------------------------------------------------------
| Firmen laden und suchen
|--------------------------------------------------------------------------
*/

const loadCompanies = async ({
    preserveMessages = false,
} = {}) => {
    loading.value = true;

    if (!preserveMessages) {
        clearMessages();
    }

    try {
        const { data } = await api.get(
            "/companies",
            {
                params: {
                    page: page.value,
                    pageSize:
                        pageSize.value,
                    search:
                        search.value.trim(),
                },
            }
        );

        companies.value =
            data.items || [];

        totalPages.value =
            data.totalPages || 1;

        totalItems.value =
            data.totalItems || 0;
    } catch (err) {
        console.error(
            "Firmen konnten nicht geladen werden:",
            err.response?.data || err
        );

        error.value =
            getApiErrorMessage(
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
    if (!canGoPrevious.value) {
        return;
    }

    page.value -= 1;
    await loadCompanies();
};

const goToNextPage = async () => {
    if (!canGoNext.value) {
        return;
    }

    page.value += 1;
    await loadCompanies();
};

/*
|--------------------------------------------------------------------------
| Firma erstellen
|--------------------------------------------------------------------------
*/

const createCompany = async () => {
    clearMessages();
    markAllCreateFieldsTouched();

    if (!isCreateFormValid.value) {
        error.value =
            "Bitte überprüfe die markierten Felder.";
        return;
    }

    creating.value = true;

    try {
        /*
         * Zuerst wird die Firma erstellt.
         * Die API sollte die erstellte Firma inklusive ID zurückgeben.
         */
        const { data: createdCompany } =
            await api.post(
                "/companies",
                {
                    name:
                        newCompany.value.name.trim(),

                    description:
                        newCompany.value.description.trim() ||
                        null,

                    websiteUrl:
                        normalizeUrl(
                            newCompany.value.websiteUrl
                        ),

                    logoUrl: null,

                    location:
                        newCompany.value.location.trim() ||
                        null,
                }
            );

        const createdCompanyId =
            createdCompany?.id ??
            createdCompany?.companyId;

        /*
         * Falls ein Logo ausgewählt wurde,
         * wird es direkt nach dem Erstellen hochgeladen.
         */
        if (newCompanyLogo.value) {
            if (!createdCompanyId) {
                throw new Error(
                    "Die Firma wurde erstellt, aber die API hat keine Firmen-ID zurückgegeben."
                );
            }

            const formData =
                new FormData();

            formData.append(
                "file",
                newCompanyLogo.value
            );

            await api.post(
                `/companies/${createdCompanyId}/logo`,
                formData,
                {
                    headers: {
                        "Content-Type":
                            "multipart/form-data",
                    },
                }
            );
        }

        page.value = 1;
        search.value = "";

        resetCreateForm();
        showCreateForm.value = false;

        await loadCompanies({
            preserveMessages: true,
        });

        success.value =
            "Firma wurde erfolgreich erstellt.";
    } catch (err) {
        console.error(
            "Firma konnte nicht erstellt werden:",
            err.response?.data || err
        );

        error.value =
            err.message ||
            getApiErrorMessage(
                err,
                "Firma konnte nicht erstellt werden."
            );
    } finally {
        creating.value = false;
    }
};

/*
|--------------------------------------------------------------------------
| Firma aktualisieren
|--------------------------------------------------------------------------
*/

const updateCompany = async (company) => {
    clearMessages();

    const companyName =
        company.name?.trim();

    if (!companyName) {
        error.value =
            "Der Firmenname darf nicht leer sein.";
        return;
    }

    updatingCompanyId.value =
        company.id;

    try {
        await api.put(
            `/companies/${company.id}`,
            {
                name: companyName,

                description:
                    company.description?.trim() ||
                    null,

                websiteUrl:
                    normalizeUrl(
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

        await loadCompanies({
            preserveMessages: true,
        });

        success.value =
            "Firma wurde aktualisiert.";
    } catch (err) {
        console.error(
            "Firma konnte nicht aktualisiert werden:",
            err.response?.data || err
        );

        error.value =
            getApiErrorMessage(
                err,
                "Firma konnte nicht aktualisiert werden."
            );
    } finally {
        updatingCompanyId.value =
            null;
    }
};

/*
|--------------------------------------------------------------------------
| Logo einer bestehenden Firma hochladen
|--------------------------------------------------------------------------
*/

const handleLogoSelected = (
    companyId,
    event
) => {
    clearMessages();

    const file =
        event.target.files?.[0];

    if (!file) {
        selectedFiles.value[
            companyId
        ] = null;

        return;
    }

    const allowedTypes = [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    const maximumFileSize =
        5 * 1024 * 1024;

    if (!allowedTypes.includes(file.type)) {
        selectedFiles.value[
            companyId
        ] = null;

        event.target.value = "";

        error.value =
            "Bitte eine JPG-, PNG- oder WebP-Datei auswählen.";

        return;
    }

    if (file.size > maximumFileSize) {
        selectedFiles.value[
            companyId
        ] = null;

        event.target.value = "";

        error.value =
            "Das Logo darf höchstens 5 MB groß sein.";

        return;
    }

    selectedFiles.value[
        companyId
    ] = file;
};

const uploadLogo = async (company) => {
    clearMessages();

    const file =
        selectedFiles.value[
        company.id
        ];

    if (!file) {
        error.value =
            "Bitte zuerst eine Logo-Datei auswählen.";

        return;
    }

    uploadingLogoCompanyId.value =
        company.id;

    try {
        const formData =
            new FormData();

        formData.append(
            "file",
            file
        );

        const { data } =
            await api.post(
                `/companies/${company.id}/logo`,
                formData,
                {
                    headers: {
                        "Content-Type":
                            "multipart/form-data",
                    },
                }
            );

        company.logoUrl =
            data.logoUrl;

        selectedFiles.value[
            company.id
        ] = null;

        await loadCompanies({
            preserveMessages: true,
        });

        success.value =
            "Logo wurde erfolgreich hochgeladen.";
    } catch (err) {
        console.error(
            "Logo konnte nicht hochgeladen werden:",
            err.response?.data || err
        );

        error.value =
            getApiErrorMessage(
                err,
                "Logo konnte nicht hochgeladen werden."
            );
    } finally {
        uploadingLogoCompanyId.value =
            null;
    }
};

/*
|--------------------------------------------------------------------------
| Eigener Löschdialog
|--------------------------------------------------------------------------
*/

const openDeleteDialog = (company) => {
    clearMessages();

    companyToDelete.value =
        company;

    showDeleteDialog.value = true;
};

const closeDeleteDialog = () => {
    if (deletingCompanyId.value) {
        return;
    }

    showDeleteDialog.value = false;
    companyToDelete.value = null;
};

const confirmDeleteCompany = async () => {
    if (!companyToDelete.value) {
        return;
    }

    clearMessages();

    const companyId =
        companyToDelete.value.id;

    deletingCompanyId.value =
        companyId;

    try {
        await api.delete(
            `/companies/${companyId}`
        );

        if (
            companies.value.length === 1 &&
            page.value > 1
        ) {
            page.value -= 1;
        }

        showDeleteDialog.value =
            false;

        companyToDelete.value =
            null;

        await loadCompanies({
            preserveMessages: true,
        });

        success.value =
            "Firma wurde gelöscht.";
    } catch (err) {
        console.error(
            "Firma konnte nicht gelöscht werden:",
            err.response?.data || err
        );

        error.value =
            getApiErrorMessage(
                err,
                "Firma konnte nicht gelöscht werden."
            );
    } finally {
        deletingCompanyId.value =
            null;
    }
};

/*
|--------------------------------------------------------------------------
| Lifecycle
|--------------------------------------------------------------------------
*/

onMounted(loadCompanies);

onBeforeUnmount(() => {
    revokeLogoPreview();
});
</script>


<template>
    <main class="container py-4">
        <template v-if="!showCreateForm">
            <PageHeader title="Firmen verwalten"
                description="Firmen erstellen, bearbeiten, löschen und mit Logos verwalten.">
                <template #actions>
                    <div class="d-grid d-sm-flex gap-2">
                        <button type="button" class="btn btn-primary" :disabled="loading" @click="openCreateForm">
                            <i class="bi bi-plus-lg me-2" aria-hidden="true"></i>
                            Neue Firma
                        </button>
                        <button type="button" class="btn btn-outline-secondary" :disabled="loading"
                            @click="loadCompanies()">
                            <span v-if="loading" class="spinner-border spinner-border-sm me-2"
                                aria-hidden="true"></span>
                            <i v-else class="bi bi-arrow-clockwise me-2" aria-hidden="true"></i>
                            Aktualisieren
                        </button>
                    </div>
                </template>
            </PageHeader>

            <div class="card border-0 shadow-sm mb-4">
                <div class="card-body">
                    <form class="row g-2 align-items-end" @submit.prevent="searchCompanies">
                        <div class="col-12 col-md">
                            <label for="companySearch" class="form-label">Firma suchen</label>
                            <input id="companySearch" v-model="search" type="search" class="form-control"
                                placeholder="Name oder Standort" :disabled="loading" />
                        </div>
                        <div class="col-12 col-md-auto">
                            <div class="d-grid d-sm-flex gap-2">
                                <button type="submit" class="btn btn-primary" :disabled="loading">Suchen</button>
                                <button type="button" class="btn btn-outline-secondary" :disabled="loading"
                                    @click="clearSearch">
                                    Zurücksetzen
                                </button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>

            <div v-if="error" class="alert alert-danger" role="alert">{{ error }}</div>
            <div v-if="success" class="alert alert-success" role="alert">{{ success }}</div>

            <BaseSpinner v-if="loading" message="Firmen werden geladen..." />

            <template v-else>
                <p class="text-body-secondary mb-3">
                    {{ totalItems }} Firmen gefunden · Seite {{ page }} von {{ totalPages }}
                </p>

                <div class="card border-0 shadow-sm">
                    <div class="card-body p-0">
                        <div v-if="hasCompanies" class="table-responsive">
                            <table class="table table-hover align-middle mb-0">
                                <thead>
                                    <tr>
                                        <th scope="col">ID</th>
                                        <th scope="col">Logo</th>
                                        <th scope="col">Name</th>
                                        <th scope="col">Standort</th>
                                        <th scope="col">Website</th>
                                        <th scope="col">Jobs</th>
                                        <th scope="col" class="text-end">Aktionen</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="company in companies" :key="company.id">
                                        <td>{{ company.id }}</td>
                                        <td>
                                            <img v-if="company.logoUrl" :src="getLogoSrc(company.logoUrl)"
                                                :alt="`${company.name} Logo`" class="img-thumbnail" width="56"
                                                height="56" />
                                            <span v-else class="badge text-bg-light">Kein Logo</span>
                                        </td>
                                        <td>
                                            <input v-model="company.name" type="text"
                                                class="form-control form-control-sm" maxlength="100" />
                                        </td>
                                        <td>
                                            <input v-model="company.location" type="text"
                                                class="form-control form-control-sm" maxlength="100" />
                                        </td>
                                        <td>
                                            <input v-model="company.websiteUrl" type="text"
                                                class="form-control form-control-sm" placeholder="https://example.com"
                                                @blur="company.websiteUrl = normalizeUrl(company.websiteUrl)" />
                                        </td>
                                        <td>{{ company.totalJobs ?? 0 }}</td>
                                        <td>
                                            <div class="d-flex flex-column flex-xl-row justify-content-end gap-2">
                                                <button type="button" class="btn btn-outline-secondary btn-sm"
                                                    :disabled="uploadingLogoCompanyId === company.id"
                                                    @click="$refs[`logoInput-${company.id}`]?.click()">
                                                    Logo wählen
                                                </button>
                                                <input :ref="`logoInput-${company.id}`" type="file" class="d-none"
                                                    accept=".jpg,.jpeg,.png,.webp"
                                                    @change="handleLogoSelected(company.id, $event)" />
                                                <button type="button" class="btn btn-outline-primary btn-sm"
                                                    :disabled="!selectedFiles[company.id] || uploadingLogoCompanyId === company.id"
                                                    @click="uploadLogo(company)">
                                                    Logo hochladen
                                                </button>
                                                <button type="button" class="btn btn-primary btn-sm"
                                                    :disabled="updatingCompanyId === company.id"
                                                    @click="updateCompany(company)">
                                                    <span v-if="updatingCompanyId === company.id"
                                                        class="spinner-border spinner-border-sm me-2"></span>
                                                    Speichern
                                                </button>
                                                <button type="button" class="btn btn-outline-danger btn-sm"
                                                    @click="openDeleteDialog(company)">
                                                    Löschen
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <BaseEmptyState v-else icon="bi-building" title="Keine Firmen vorhanden"
                            message="Für die aktuelle Suche wurden keine Firmen gefunden." />
                    </div>

                    <div v-if="hasCompanies" class="card-footer bg-body border-top">
                        <BasePagination :page="page" :total-pages="totalPages" :can-go-previous="canGoPrevious"
                            :can-go-next="canGoNext" @previous="goToPreviousPage" @next="goToNextPage" />
                    </div>
                </div>
            </template>
        </template>

        <template v-else>
            <PageHeader title="Neue Firma erstellen"
                description="Firmendaten erfassen und optional ein Logo hochladen.">
                <template #actions>
                    <button type="button" class="btn btn-outline-secondary" :disabled="creating"
                        @click="closeCreateForm">
                        <i class="bi bi-arrow-left me-2" aria-hidden="true"></i>
                        Zur Firmenübersicht
                    </button>
                </template>
            </PageHeader>

            <div v-if="error" class="alert alert-danger" role="alert">{{ error }}</div>

            <div class="card border-0 shadow-sm">
                <div class="card-body p-4">
                    <form novalidate @submit.prevent="createCompany">
                        <div class="row g-4">
                            <div class="col-12 col-lg-8">
                                <div class="row g-3">
                                    <div class="col-12 col-md-6">
                                        <label for="companyName" class="form-label">Firmenname *</label>
                                        <input id="companyName" v-model="newCompany.name" type="text"
                                            class="form-control"
                                            :class="{ 'is-invalid': createFormTouched.name && createFormErrors.name }"
                                            maxlength="100" :disabled="creating"
                                            @blur="markCreateFieldTouched('name')" />
                                        <div class="invalid-feedback">{{ createFormErrors.name }}</div>
                                    </div>

                                    <div class="col-12 col-md-6">
                                        <label for="companyLocation" class="form-label">Standort</label>
                                        <input id="companyLocation" v-model="newCompany.location" type="text"
                                            class="form-control"
                                            :class="{ 'is-invalid': createFormTouched.location && createFormErrors.location }"
                                            maxlength="100" :disabled="creating"
                                            @blur="markCreateFieldTouched('location')" />
                                        <div class="invalid-feedback">{{ createFormErrors.location }}</div>
                                    </div>

                                    <div class="col-12">
                                        <label for="companyWebsite" class="form-label">Website</label>
                                        <input id="companyWebsite" v-model="newCompany.websiteUrl" type="text"
                                            class="form-control"
                                            :class="{ 'is-invalid': createFormTouched.websiteUrl && createFormErrors.websiteUrl }"
                                            placeholder="example.com" :disabled="creating"
                                            @blur="handleCreateWebsiteBlur" />
                                        <div v-if="createFormTouched.websiteUrl && createFormErrors.websiteUrl"
                                            class="invalid-feedback">
                                            {{ createFormErrors.websiteUrl }}
                                        </div>
                                        <div v-else class="form-text">https:// wird automatisch ergänzt.</div>
                                    </div>

                                    <div class="col-12">
                                        <label for="companyDescription" class="form-label">Beschreibung</label>
                                        <textarea id="companyDescription" v-model="newCompany.description"
                                            class="form-control"
                                            :class="{ 'is-invalid': createFormTouched.description && createFormErrors.description }"
                                            rows="7" maxlength="1000" :disabled="creating"
                                            @blur="markCreateFieldTouched('description')"></textarea>
                                        <div v-if="createFormTouched.description && createFormErrors.description"
                                            class="invalid-feedback">
                                            {{ createFormErrors.description }}
                                        </div>
                                        <div v-else class="form-text text-end">{{ newCompany.description.length }} /
                                            1000 Zeichen
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 col-lg-4">
                                <div class="card bg-body-tertiary border-0 h-100">
                                    <div class="card-body">
                                        <label for="newCompanyLogo" class="form-label">Firmenlogo</label>
                                        <div class="ratio ratio-1x1 bg-body border rounded mb-3 overflow-hidden">
                                            <img v-if="newCompanyLogoPreview" :src="newCompanyLogoPreview"
                                                alt="Vorschau des Firmenlogos"
                                                class="w-100 h-100 object-fit-contain p-3" />
                                            <div v-else
                                                class="d-flex flex-column justify-content-center align-items-center text-body-secondary">
                                                <i class="bi bi-image fs-1" aria-hidden="true"></i>
                                                <span class="small">Kein Logo ausgewählt</span>
                                            </div>
                                        </div>
                                        <input id="newCompanyLogo" type="file" accept=".jpg,.jpeg,.png,.webp"
                                            class="form-control"
                                            :class="{ 'is-invalid': createFormTouched.logo && createFormErrors.logo }"
                                            :disabled="creating" @change="handleNewCompanyLogoSelected" />
                                        <div v-if="createFormTouched.logo && createFormErrors.logo"
                                            class="invalid-feedback">
                                            {{ createFormErrors.logo }}
                                        </div>
                                        <div v-else class="form-text">JPG, PNG oder WebP, maximal 5 MB.</div>
                                        <button v-if="newCompanyLogo" type="button"
                                            class="btn btn-outline-danger btn-sm mt-3" :disabled="creating"
                                            @click="removeNewCompanyLogo">
                                            Logo entfernen
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <hr class="my-4" />
                        <div class="d-grid d-sm-flex justify-content-sm-end gap-2">
                            <button type="button" class="btn btn-outline-secondary" :disabled="creating"
                                @click="closeCreateForm">
                                Abbrechen
                            </button>
                            <button type="submit" class="btn btn-primary" :disabled="creating || !isCreateFormValid">
                                <span v-if="creating" class="spinner-border spinner-border-sm me-2"
                                    aria-hidden="true"></span>
                                {{ creating ? "Firma wird erstellt..." : "Firma erstellen" }}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </template>

        <template v-if="showDeleteDialog">
            <div class="modal d-block" tabindex="-1" role="dialog" aria-modal="true"
                aria-labelledby="deleteDialogTitle">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h2 id="deleteDialogTitle" class="modal-title h5">Firma löschen?</h2>
                            <button type="button" class="btn-close" :disabled="deletingCompanyId !== null"
                                @click="closeDeleteDialog"></button>
                        </div>
                        <div class="modal-body">
                            <p>Möchtest du die Firma <strong>{{ companyToDelete?.name }}</strong> wirklich löschen?</p>
                            <p class="text-body-secondary mb-0">Diese Aktion kann nicht rückgängig gemacht werden.</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-outline-secondary"
                                :disabled="deletingCompanyId !== null" @click="closeDeleteDialog">
                                Abbrechen
                            </button>
                            <button type="button" class="btn btn-danger" :disabled="deletingCompanyId !== null"
                                @click="confirmDeleteCompany">
                                <span v-if="deletingCompanyId !== null" class="spinner-border spinner-border-sm me-2"
                                    aria-hidden="true"></span>
                                {{ deletingCompanyId !== null ? "Wird gelöscht..." : "Firma löschen" }}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
            <div class="modal-backdrop fade show"></div>
        </template>
    </main>
</template>
