<script setup>
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import PageHeader from "@/components/shared/PageHeader.vue";
import { USER_ROLES } from "@/constants/roles";
import { clearLocalSession } from "@/services/api";
import {
    deleteAccount,
    downloadBlobFile,
    exportPersonalData,
} from "@/services/userService";
import { getCurrentUser } from "@/utils/storage";

const router = useRouter();

const currentUser = ref(getCurrentUser());

const exportLoading = ref(false);
const deleteLoading = ref(false);

const exportSuccess = ref("");
const exportError = ref("");

const deleteError = ref("");

const password = ref("");
const confirmDeletion = ref(false);
const showPassword = ref(false);

const isAdmin = computed(
    () => currentUser.value?.role === USER_ROLES.ADMIN,
);

const canDeleteAccount = computed(() => {
    return (
        password.value.trim().length > 0 &&
        confirmDeletion.value &&
        !deleteLoading.value
    );
});

const clearExportMessages = () => {
    exportSuccess.value = "";
    exportError.value = "";
};

const clearDeleteMessages = () => {
    deleteError.value = "";
};

const getApiErrorMessage = (
    error,
    fallbackMessage,
) => {
    const responseData = error?.response?.data;

    if (typeof responseData?.message === "string") {
        return responseData.message;
    }

    /*
     * Falls das Backend bei einem Fehler JSON als Blob zurückgibt,
     * kann Axios die Meldung nicht direkt als Objekt lesen.
     */
    if (responseData instanceof Blob) {
        return fallbackMessage;
    }

    return fallbackMessage;
};

const handleExport = async () => {
    exportLoading.value = true;
    clearExportMessages();

    try {
        const { blob, fileName } =
            await exportPersonalData();

        downloadBlobFile(blob, fileName);

        exportSuccess.value =
            "Deine persönlichen Daten wurden erfolgreich heruntergeladen.";
    } catch (error) {
        exportError.value = getApiErrorMessage(
            error,
            "Die persönlichen Daten konnten nicht heruntergeladen werden.",
        );
    } finally {
        exportLoading.value = false;
    }
};

const handleDeleteAccount = async () => {
    clearDeleteMessages();

    if (isAdmin.value) {
        deleteError.value =
            "Administratorkonten können nicht über diese Seite gelöscht werden.";

        return;
    }

    if (!password.value.trim()) {
        deleteError.value =
            "Bitte gib dein aktuelles Passwort ein.";

        return;
    }

    if (!confirmDeletion.value) {
        deleteError.value =
            "Bitte bestätige, dass du dein Konto dauerhaft löschen möchtest.";

        return;
    }

    const userConfirmed = window.confirm(
        "Möchtest du dein Konto wirklich dauerhaft löschen? " +
        "Diese Aktion kann nicht rückgängig gemacht werden.",
    );

    if (!userConfirmed) {
        return;
    }

    deleteLoading.value = true;

    try {
        await deleteAccount({
            password: password.value,
            confirmDeletion: true,
        });

        /*
         * Nach erfolgreicher Löschung entfernen wir Access Token
         * und Benutzerdaten aus dem Local Storage.
         *
         * Das Refresh-Cookie wird vom Backend entfernt.
         */
        clearLocalSession();

        await router.replace({
            path: "/login",
            query: {
                accountDeleted: "true",
            },
        });
    } catch (error) {
        deleteError.value = getApiErrorMessage(
            error,
            "Das Konto konnte nicht gelöscht werden.",
        );
    } finally {
        deleteLoading.value = false;
    }
};
</script>

<template>
    <main class="container py-4">
        <PageHeader title="Datenschutz und Konto"
            description="Lade deine gespeicherten Daten herunter oder verwalte die dauerhafte Löschung deines Kontos." />

        <section class="card border-0 shadow-sm mb-4">
            <div class="card-body p-4">
                <div class="d-flex flex-column flex-lg-row justify-content-between align-items-lg-start gap-4">
                    <div>
                        <h2 class="h5 mb-2">
                            Persönliche Daten exportieren
                        </h2>

                        <p class="text-body-secondary mb-2">
                            Du erhältst eine ZIP-Datei mit deinen gespeicherten persönlichen Daten.
                        </p>

                        <p class="small text-body-secondary mb-0">
                            Die Datei kann unter anderem Profildaten, Bewerbungen, Skills,
                            Kurse und Karriereziele enthalten.
                        </p>
                    </div>

                    <div class="d-grid d-sm-block flex-shrink-0">
                        <button type="button" class="btn btn-primary" :disabled="exportLoading" @click="handleExport">
                            <span v-if="exportLoading" class="spinner-border spinner-border-sm me-2"
                                aria-hidden="true" />

                            {{
                                exportLoading
                                    ? "Export wird erstellt..."
                                    : "Daten herunterladen"
                            }}
                        </button>
                    </div>
                </div>

                <div v-if="exportSuccess || exportError" class="mt-3">
                    <BaseAlert type="success" :message="exportSuccess" />

                    <BaseAlert type="danger" :message="exportError" />
                </div>
            </div>
        </section>

        <section class="card border-danger shadow-sm">
            <div class="card-header bg-danger-subtle border-danger p-4">
                <h2 class="h5 text-danger mb-1">
                    Gefahrenbereich
                </h2>

                <p class="text-body-secondary mb-0">
                    Die Kontolöschung ist dauerhaft und kann nicht rückgängig gemacht werden.
                </p>
            </div>

            <div class="card-body p-4">
                <template v-if="isAdmin">
                    <BaseAlert type="warning"
                        message="Administratorkonten können nicht über diese Seite gelöscht werden." />
                </template>

                <template v-else>
                    <p class="mb-4">
                        Beim Löschen werden dein Konto und die damit verbundenen persönlichen
                        Daten entfernt. Dazu können unter anderem Bewerbungen, Skills,
                        Kursfortschritte, Lebenslauf und Profilbild gehören.
                    </p>

                    <div class="row g-3">
                        <div class="col-12 col-lg-8">
                            <label for="delete-account-password" class="form-label fw-semibold">
                                Aktuelles Passwort
                            </label>

                            <div class="input-group">
                                <input id="delete-account-password" v-model="password"
                                    :type="showPassword ? 'text' : 'password'" class="form-control"
                                    autocomplete="current-password" placeholder="Aktuelles Passwort eingeben"
                                    :disabled="deleteLoading" @input="clearDeleteMessages" />

                                <button type="button" class="btn btn-outline-secondary" :disabled="deleteLoading"
                                    :aria-label="showPassword ? 'Passwort ausblenden' : 'Passwort anzeigen'"
                                    @click="showPassword = !showPassword">
                                    <i class="bi" :class="showPassword ? 'bi-eye-slash' : 'bi-eye'"
                                        aria-hidden="true" />
                                    <span class="d-none d-sm-inline ms-1">
                                        {{ showPassword ? "Ausblenden" : "Anzeigen" }}
                                    </span>
                                </button>
                            </div>
                        </div>

                        <div class="col-12">
                            <div class="form-check">
                                <input id="confirm-account-deletion" v-model="confirmDeletion" class="form-check-input"
                                    type="checkbox" :disabled="deleteLoading" @change="clearDeleteMessages" />

                                <label class="form-check-label" for="confirm-account-deletion">
                                    Ich verstehe, dass mein Konto und meine Daten dauerhaft gelöscht werden.
                                </label>
                            </div>
                        </div>
                    </div>

                    <div v-if="deleteError" class="mt-3">
                        <BaseAlert type="danger" :message="deleteError" />
                    </div>

                    <div class="d-grid d-sm-block mt-4">
                        <button type="button" class="btn btn-danger" :disabled="!canDeleteAccount"
                            @click="handleDeleteAccount">
                            <span v-if="deleteLoading" class="spinner-border spinner-border-sm me-2"
                                aria-hidden="true" />

                            {{
                                deleteLoading
                                    ? "Konto wird gelöscht..."
                                    : "Konto dauerhaft löschen"
                            }}
                        </button>
                    </div>
                </template>
            </div>
        </section>
    </main>
</template>
