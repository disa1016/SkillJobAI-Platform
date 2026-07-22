<script setup>
import { ref } from "vue";
import { forgotPassword } from "@/services/authService";
import BaseAlert from "@/components/shared/BaseAlert.vue";

const email = ref("");
const loading = ref(false);
const error = ref("");
const success = ref("");

const handleForgotPassword = async () => {
  error.value = "";
  success.value = "";
  loading.value = true;

  try {
    const data = await forgotPassword({
      email: email.value.trim(),
    });

    success.value =
      data?.message ||
      "Falls ein Konto mit dieser E-Mail-Adresse existiert, wurde eine E-Mail zum Zurücksetzen des Passworts versendet.";

    email.value = "";
  } catch (err) {
    error.value =
      err.response?.data?.message ||
      "Die Anfrage konnte nicht verarbeitet werden.";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <main class="container py-5">
    <div class="row justify-content-center">
      <div class="col-12 col-sm-10 col-md-7 col-lg-5">
        <div class="card border-0 shadow-sm">
          <div class="card-body p-4 p-md-5">
            <div class="text-center mb-4">
              <i class="bi bi-key display-5 text-primary" aria-hidden="true"></i>
              <h1 class="h3 mt-3 mb-1">Passwort vergessen</h1>
              <p class="text-body-secondary mb-0">
                Gib deine E-Mail-Adresse ein. Wir senden dir weitere Anweisungen.
              </p>
            </div>

            <BaseAlert v-if="error" type="danger" :message="error" />
            <BaseAlert v-if="success" type="success" :message="success" />

            <form @submit.prevent="handleForgotPassword">
              <div class="mb-4">
                <label for="forgot-password-email" class="form-label">E-Mail</label>
                <input
                  id="forgot-password-email"
                  v-model="email"
                  type="email"
                  class="form-control"
                  autocomplete="email"
                  required
                />
              </div>

              <div class="d-grid">
                <button type="submit" class="btn btn-primary" :disabled="loading">
                  <span
                    v-if="loading"
                    class="spinner-border spinner-border-sm me-2"
                    aria-hidden="true"
                  ></span>
                  {{ loading ? "Anfrage läuft..." : "Reset-Link anfordern" }}
                </button>
              </div>
            </form>

            <p class="text-center mt-4 mb-0">
              <router-link to="/login">
                <i class="bi bi-arrow-left me-1" aria-hidden="true"></i>
                Zurück zur Anmeldung
              </router-link>
            </p>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>
