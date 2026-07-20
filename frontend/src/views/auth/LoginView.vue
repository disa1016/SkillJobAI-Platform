<script setup>
import { ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { login } from "@/services/authService";
import BaseAlert from "@/components/shared/BaseAlert.vue";

const route = useRoute();
const router = useRouter();

const email = ref("");
const password = ref("");

const loading = ref(false);
const error = ref("");

const getHomePathByRole = (role) => {
  if (role === "Admin") {
    return "/admin/dashboard";
  }

  if (role === "Recruiter") {
    return "/recruiter/dashboard";
  }

  return "/dashboard";
};

const getSafeRedirectPath = (user) => {
  const requestedPath =
    typeof route.query.redirect === "string"
      ? route.query.redirect
      : null;

  /*
   * Es werden nur interne relative Pfade erlaubt.
   * Dadurch verhindern wir offene Weiterleitungen.
   */
  if (
    requestedPath &&
    requestedPath.startsWith("/") &&
    !requestedPath.startsWith("//")
  ) {
    return requestedPath;
  }

  return getHomePathByRole(user?.role);
};

const handleLogin = async () => {
  error.value = "";
  loading.value = true;

  try {
    const data = await login({
      email: email.value,
      password: password.value,
    });

    await router.replace(
      getSafeRedirectPath(data.user)
    );
  } catch (err) {
    error.value =
      err.response?.data?.message ||
      "E-Mail oder Passwort ist falsch.";
  } finally {
    loading.value = false;
  }
};
</script>

<template>
    <div class="container min-vh-100 d-flex align-items-center justify-content-center">
        <div class="card shadow p-4" style="max-width: 420px; width: 100%">
            <h2 class="text-center text-primary mb-3">
                SkillJob AI
            </h2>

            <p class="text-center text-muted">
                Login to your account
            </p>

            <BaseAlert type="danger" :message="error" />

            <form @submit.prevent="handleLogin">
                <div class="mb-3">
                    <label class="form-label">E-Mail</label>

                    <input v-model="email" type="email" class="form-control" autocomplete="email" required />
                </div>

                <div class="mb-3">
                    <label class="form-label">Passwort</label>

                    <input v-model="password" type="password" class="form-control" autocomplete="current-password"
                        required />
                </div>

                <button type="submit" class="btn btn-primary w-100" :disabled="loading">
                    {{ loading ? "Bitte warten..." : "Login" }}
                </button>
            </form>

            <p class="text-center mt-3 mb-0">
                Noch kein Konto?
                <router-link to="/register">
                    Registrieren
                </router-link>
            </p>

            <p class="text-center mt-3">
                <router-link to="/forgot-password">
                    Passwort vergessen?
                </router-link>
            </p>
        </div>
    </div>
</template>