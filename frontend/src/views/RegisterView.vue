<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import api from "../services/api";

const router = useRouter();

const fullName = ref("");
const email = ref("");
const password = ref("");
const role = ref("Student");
const error = ref("");
const loading = ref(false);

const register = async () => {
  error.value = "";
  loading.value = true;

  try {
    await api.post("/auth/register", {
      fullName: fullName.value,
      email: email.value,
      password: password.value,
      role: role.value,
    });

    router.push("/login");
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
    <div class="card shadow p-4" style="max-width: 460px; width: 100%;">
      <h2 class="text-center text-primary mb-3">SkillJob AI</h2>
      <p class="text-center text-muted">Create your account</p>

      <div v-if="error" class="alert alert-danger">
        {{ error }}
      </div>

      <div class="mb-3">
        <label class="form-label">Name</label>
        <input v-model="fullName" type="text" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">E-Mail</label>
        <input v-model="email" type="email" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">Passwort</label>
        <input v-model="password" type="password" class="form-control" />
      </div>

      <div class="mb-3">
        <label class="form-label">Account-Typ</label>
        <select v-model="role" class="form-select">
          <option value="Student">Student </option>
          <option value="Candidate">Candidate</option>
          <option value="Recruiter">Recruiter </option>
          <option value=" Employer"> Employer</option>
        </select>
      </div>

      <button class="btn btn-primary w-100" @click="register" :disabled="loading">
        {{ loading ? "Bitte warten..." : "Registrieren" }}
      </button>

      <p class="text-center mt-3 mb-0">
        Schon ein Konto?
        <router-link to="/login">Login</router-link>
      </p>
    </div>
  </div>
</template>