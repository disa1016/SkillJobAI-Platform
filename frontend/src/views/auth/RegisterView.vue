<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { register } from "@/services/authService";
import BaseAlert from "@/components/shared/BaseAlert.vue";


const router = useRouter();

const fullName = ref("");
const email = ref("");
const password = ref("");
const role = ref("Student");

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

    success.value =
      "Registrierung erfolgreich.";

    await router.replace(
      data.user?.role === "Admin"
        ? "/admin/dashboard"
        : data.user?.role === "Recruiter"
          ? "/recruiter/dashboard"
          : "/dashboard"
    );

    success.value = "Registrierung erfolgreich.";

    
  } catch (err) {
    error.value =
      err.response?.data?.message || "Registrierung fehlgeschlagen.";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="container min-vh-100 d-flex align-items-center justify-content-center">
    <div class="card shadow p-4" style="max-width: 460px; width: 100%">
      <h2 class="text-center text-primary mb-3">
        SkillJob AI
      </h2>

      <p class="text-center text-muted">
        Create your account
      </p>

      <BaseAlert v-if="error" type="danger" :message="error" />

      <BaseAlert v-if="success" type="success" :message="success" />

      <form @submit.prevent="handleRegister">
        <div class="mb-3">
          <label class="form-label">Name</label>

          <input v-model="fullName" type="text" class="form-control" autocomplete="name" required />
        </div>

        <div class="mb-3">
          <label class="form-label">E-Mail</label>

          <input v-model="email" type="email" class="form-control" autocomplete="email" required />
        </div>

        <div class="mb-3">
          <label class="form-label">Passwort</label>

          <input v-model="password" type="password" class="form-control" autocomplete="new-password" required />
        </div>

     
        <button type="submit" class="btn btn-primary w-100" :disabled="loading">
          {{ loading ? "Bitte warten..." : "Registrieren" }}
        </button>
      </form>

      <p class="text-center mt-3 mb-0">
        Schon ein Konto?
        <router-link to="/login">
          Login
        </router-link>
      </p>
    </div>
  </div>
</template>