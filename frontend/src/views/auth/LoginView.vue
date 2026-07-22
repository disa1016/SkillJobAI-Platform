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
    if (role === "Admin") return "/admin/dashboard";
    if (role === "Recruiter") return "/recruiter/dashboard";
    return "/dashboard";
};

const getSafeRedirectPath = (user) => {
    const requestedPath =
        typeof route.query.redirect === "string" ? route.query.redirect : null;

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

        await router.replace(getSafeRedirectPath(data.user));
    } catch (err) {
        error.value =
            err.response?.data?.message || "E-Mail oder Passwort ist falsch.";
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <main class="container py-5">
        <div class="row justify-content-center">
            <div class="col-12 col-sm-10 col-md-7 col-lg-5 col-xl-4">
                <div class="card border-0 shadow-sm">
                    <div class="card-body p-4 p-md-5">
                        <div class="text-center mb-4">
                            <i class="bi bi-person-circle display-5 text-primary" aria-hidden="true"></i>
                            <h1 class="h3 mt-3 mb-1">Anmelden</h1>
                            <p class="text-body-secondary mb-0">
                                Melde dich bei SkillJob AI an.
                            </p>
                        </div>

                        <BaseAlert v-if="error" type="danger" :message="error" />

                        <form @submit.prevent="handleLogin">
                            <div class="mb-3">
                                <label for="login-email" class="form-label">E-Mail</label>
                                <input id="login-email" v-model="email" type="email" class="form-control"
                                    autocomplete="email" required />
                            </div>

                            <div class="mb-4">
                                <div class="d-flex justify-content-between align-items-center gap-2">
                                    <label for="login-password" class="form-label">Passwort</label>
                                    <router-link to="/forgot-password" class="small">
                                        Passwort vergessen?
                                    </router-link>
                                </div>
                                <input id="login-password" v-model="password" type="password" class="form-control"
                                    autocomplete="current-password" required />
                            </div>

                            <div class="d-grid">
                                <button type="submit" class="btn btn-primary" :disabled="loading">
                                    <span v-if="loading" class="spinner-border spinner-border-sm me-2"
                                        aria-hidden="true"></span>
                                    {{ loading ? "Anmeldung läuft..." : "Anmelden" }}
                                </button>
                            </div>
                        </form>

                        <p class="text-center text-body-secondary mt-4 mb-0">
                            Noch kein Konto?
                            <router-link to="/register">Registrieren</router-link>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>
