<script setup>
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

import BaseAlert from "@/components/shared/BaseAlert.vue";
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
  <main class="container py-4 py-lg-5">
    <div class="account-page-header mb-4">
      <p class="text-uppercase text-primary fw-semibold small mb-2">
        Kontoeinstellungen
      </p>

      <h1 class="h2 mb-2">
        Datenschutz und Konto
      </h1>

      <p class="text-secondary mb-0">
        Lade deine gespeicherten Daten herunter oder verwalte
        die dauerhafte Löschung deines Kontos.
      </p>
    </div>

    <section class="card border-0 shadow-sm mb-4">
      <div class="card-body p-4">
        <div
          class="d-flex flex-column flex-lg-row justify-content-between gap-4"
        >
          <div>
            <h2 class="h5 mb-2">
              Persönliche Daten exportieren
            </h2>

            <p class="text-secondary mb-2">
              Du erhältst eine ZIP-Datei mit deinen gespeicherten
              persönlichen Daten.
            </p>

            <p class="small text-secondary mb-0">
              Die Datei kann unter anderem Profildaten,
              Bewerbungen, Skills, Kurse und Karriereziele enthalten.
            </p>
          </div>

          <div class="flex-shrink-0">
            <button
              type="button"
              class="btn btn-primary"
              :disabled="exportLoading"
              @click="handleExport"
            >
              <span
                v-if="exportLoading"
                class="spinner-border spinner-border-sm me-2"
                aria-hidden="true"
              />

              {{
                exportLoading
                  ? "Export wird erstellt..."
                  : "Daten herunterladen"
              }}
            </button>
          </div>
        </div>

        <div class="mt-3">
          <BaseAlert
            type="success"
            :message="exportSuccess"
          />

          <BaseAlert
            type="danger"
            :message="exportError"
          />
        </div>
      </div>
    </section>

    <section class="card border-danger shadow-sm">
      <div class="card-header bg-danger-subtle border-danger p-4">
        <h2 class="h5 text-danger mb-1">
          Gefahrenbereich
        </h2>

        <p class="text-secondary mb-0">
          Die Kontolöschung ist dauerhaft und kann nicht
          rückgängig gemacht werden.
        </p>
      </div>

      <div class="card-body p-4">
        <template v-if="isAdmin">
          <BaseAlert
            type="warning"
            message="Administratorkonten können nicht über diese Seite gelöscht werden."
          />
        </template>

        <template v-else>
          <p class="mb-3">
            Beim Löschen werden dein Konto und die damit verbundenen
            persönlichen Daten entfernt. Dazu können unter anderem
            Bewerbungen, Skills, Kursfortschritte, Lebenslauf und
            Profilbild gehören.
          </p>

          <div class="mb-3">
            <label
              for="delete-account-password"
              class="form-label fw-semibold"
            >
              Aktuelles Passwort
            </label>

            <div class="input-group">
              <input
                id="delete-account-password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                class="form-control"
                autocomplete="current-password"
                placeholder="Aktuelles Passwort eingeben"
                :disabled="deleteLoading"
                @input="clearDeleteMessages"
              />

              <button
                type="button"
                class="btn btn-outline-secondary"
                :disabled="deleteLoading"
                @click="showPassword = !showPassword"
              >
                {{
                  showPassword
                    ? "Ausblenden"
                    : "Anzeigen"
                }}
              </button>
            </div>
          </div>

          <div class="form-check mb-4">
            <input
              id="confirm-account-deletion"
              v-model="confirmDeletion"
              class="form-check-input"
              type="checkbox"
              :disabled="deleteLoading"
              @change="clearDeleteMessages"
            />

            <label
              class="form-check-label"
              for="confirm-account-deletion"
            >
              Ich verstehe, dass mein Konto und meine Daten
              dauerhaft gelöscht werden.
            </label>
          </div>

          <BaseAlert
            type="danger"
            :message="deleteError"
          />

          <button
            type="button"
            class="btn btn-danger"
            :disabled="!canDeleteAccount"
            @click="handleDeleteAccount"
          >
            <span
              v-if="deleteLoading"
              class="spinner-border spinner-border-sm me-2"
              aria-hidden="true"
            />

            {{
              deleteLoading
                ? "Konto wird gelöscht..."
                : "Konto dauerhaft löschen"
            }}
          </button>
        </template>
      </div>
    </section>
  </main>
</template>

<style scoped>
.account-page-header {
  max-width: 760px;
}

.card {
  border-radius: 1rem;
}

.card-header:first-child {
  border-radius: 1rem 1rem 0 0;
}

.form-control,
.input-group .btn {
  min-height: 46px;
}

@media (max-width: 575.98px) {
  .card-body,
  .card-header {
    padding: 1.25rem !important;
  }

  .btn {
    width: 100%;
  }

  .input-group .btn {
    width: auto;
  }
}
</style>