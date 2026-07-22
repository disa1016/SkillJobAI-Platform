<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { register } from "@/services/authService";
import BaseAlert from "@/components/shared/BaseAlert.vue";

const router = useRouter();

const fullName = ref("");
const email = ref("");
const password = ref("");
const loading = ref(false);
const error = ref("");
const success = ref("");

const handleRegister = async () => {
  error.value = "";
  success.value = "";
  loading.value = true;

  try {
    const data = await register({
      fullName: fullName.value,
      email: email.value,
      password: password.value,
    });

    success.value = "Registrierung erfolgreich.";

    await router.replace(
      data.user?.role === "Admin"
        ? "/admin/dashboard"
        : data.user?.role === "Recruiter"
          ? "/recruiter/dashboard"
          : "/dashboard"
    );
  } catch (err) {
    error.value =
      err.response?.data?.message || "Registrierung fehlgeschlagen.";
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
              <i class="bi bi-person-plus display-5 text-primary" aria-hidden="true"></i>
              <h1 class="h3 mt-3 mb-1">Konto erstellen</h1>
              <p class="text-body-secondary mb-0">
                Registriere dich bei SkillJob AI.
              </p>
            </div>

            <BaseAlert v-if="error" type="danger" :message="error" />
            <BaseAlert v-if="success" type="success" :message="success" />

            <form @submit.prevent="handleRegister">
              <div class="mb-3">
                <label for="register-name" class="form-label">Name</label>
                <input id="register-name" v-model="fullName" type="text" class="form-control" autocomplete="name"
                  required />
              </div>

              <div class="mb-3">
                <label for="register-email" class="form-label">E-Mail</label>
                <input id="register-email" v-model="email" type="email" class="form-control" autocomplete="email"
                  required />
              </div>

              <div class="mb-4">
                <label for="register-password" class="form-label">Passwort</label>
                <input id="register-password" v-model="password" type="password" class="form-control"
                  autocomplete="new-password" required />
              </div>

              <div class="d-grid">
                <button type="submit" class="btn btn-primary" :disabled="loading">
                  <span v-if="loading" class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>
                  {{ loading ? "Registrierung läuft..." : "Registrieren" }}
                </button>
              </div>
            </form>

            <p class="text-center text-body-secondary mt-4 mb-0">
              Schon ein Konto?
              <router-link to="/login">Anmelden</router-link>
            </p>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>
