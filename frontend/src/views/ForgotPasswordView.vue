<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import api from "../services/api";

const router = useRouter();

const email = ref("");
const newPassword = ref("");
const error = ref("");
const success = ref("");
const loading = ref(false);

const resetPassword = async () => {
  error.value = "";
  success.value = "";
  loading.value = true;

  try {
    await api.post("/auth/forgot-password", {
      email: email.value,
      newPassword: newPassword.value,
    });

    success.value = "Passwort wurde erfolgreich geändert.";

    setTimeout(() => {
      router.push("/login");
    }, 1200);
  } catch (err) {
    error.value =
      err.response?.data?.message || "Passwort konnte nicht geändert werden.";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="container min-vh-100 d-flex align-items-center justify-content-center">
    <div class="card shadow p-4" style="max-width: 460px; width: 100%;">
      <h2 class="text-center text-primary mb-3">Passwort vergessen</h2>

      <div v-if="error" class="alert alert-danger">
        {{ error }}
      </div>

      <div v-if="success" class="alert alert-success">
        {{ success }}
      </div>

      <div class="mb-3">
        <label class="form-label">E-Mail</label>
        <input v-model="email" type="email" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">Neues Passwort</label>
        <input v-model="newPassword" type="password" class="form-control" />
      </div>

      <button class="btn btn-primary w-100" @click="resetPassword" :disabled="loading">
        {{ loading ? "Bitte warten..." : "Passwort zurücksetzen" }}
      </button>

      <p class="text-center mt-3 mb-0">
        <router-link to="/login">Zurück zum Login</router-link>
      </p>
    </div>
  </div>
</template>