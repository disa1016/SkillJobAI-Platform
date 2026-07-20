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
  <div class="container min-vh-100 d-flex align-items-center justify-content-center">
    <div class="card shadow p-4" style="max-width: 460px; width: 100%">
      <h2 class="text-center text-primary mb-3">
        Passwort vergessen
      </h2>

      <BaseAlert type="danger" :message="error" />

      <BaseAlert type="success" :message="success" />

      <form @submit.prevent="handleResetPassword">
        <div class="mb-3">
          <label class="form-label">E-Mail</label>

          <input v-model="email" type="email" class="form-control" autocomplete="email" required />
        </div>

        <div class="mb-3">
          <label class="form-label">Neues Passwort</label>

          <input v-model="newPassword" type="password" class="form-control" autocomplete="new-password" required />
        </div>

        <button type="submit" class="btn btn-primary w-100" :disabled="loading">
          {{ loading ? "Bitte warten..." : "Passwort zurücksetzen" }}
        </button>
      </form>

      <p class="text-center mt-3 mb-0">
        <router-link to="/login">
          Zurück zum Login
        </router-link>
      </p>
    </div>
  </div>
</template>