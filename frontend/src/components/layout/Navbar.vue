<script setup>
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

import { USER_ROLES } from "@/constants/roles";
import { logout as logoutUser } from "@/services/authService";
import { clearAuthStorage, getCurrentUser } from "@/utils/storage";

const router = useRouter();

const user = ref(getCurrentUser());
const logoutLoading = ref(false);

const isAdmin = computed(
  () => user.value?.role === USER_ROLES.ADMIN,
);

const isRecruiter = computed(
  () => user.value?.role === USER_ROLES.RECRUITER,
);

const isCandidate = computed(
  () =>
    user.value?.role === USER_ROLES.CANDIDATE ||
    user.value?.role === USER_ROLES.STUDENT,
);

const homePath = computed(() => {
  if (!user.value) {
    return "/home";
  }

  if (isAdmin.value) {
    return "/admin/dashboard";
  }

  if (isRecruiter.value) {
    return "/recruiter/dashboard";
  }

  return "/dashboard";
});

const displayedRole = computed(() => {
  if (isCandidate.value) {
    return "Candidate";
  }

  return user.value?.role || "";
});

const logout = async () => {
  if (logoutLoading.value) {
    return;
  }

  logoutLoading.value = true;

  try {
    await logoutUser();
  } catch (error) {
    console.error("Logout failed:", error);
  } finally {
    clearAuthStorage();
    user.value = null;
    logoutLoading.value = false;

    await router.replace("/login");
  }
};
</script>

<template>
  <nav class="navbar navbar-expand-xl bg-body border-bottom shadow-sm">
    <div class="container-fluid px-3 px-lg-4">
      <router-link
        class="navbar-brand d-inline-flex align-items-center gap-2 fw-semibold"
        :to="homePath"
      >
        <i class="bi bi-mortarboard-fill fs-4 text-primary" aria-hidden="true"></i>
        <span>SkillJob AI</span>
      </router-link>

      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#mainNavbar"
        aria-controls="mainNavbar"
        aria-expanded="false"
        aria-label="Navigation öffnen"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <div id="mainNavbar" class="collapse navbar-collapse">
        <div class="navbar-nav ms-auto align-items-xl-center gap-xl-1 py-3 py-xl-0">
          <template v-if="!user">
            <router-link to="/home" class="btn btn-outline-primary btn-sm">
              Startseite
            </router-link>
          </template>

          <template v-else>
            <template v-if="isCandidate">
              <router-link class="nav-link" to="/dashboard">
                Dashboard
              </router-link>

              <router-link class="nav-link" to="/career-roadmap">
                Roadmap
              </router-link>

              <router-link class="nav-link" to="/courses">
                Kurse
              </router-link>

              <router-link class="nav-link" to="/jobs">
                Jobs
              </router-link>

              <router-link class="nav-link" to="/profile/skills">
                Skills
              </router-link>

              <router-link class="nav-link" to="/my-applications">
                Bewerbungen
              </router-link>

              <div class="nav-item dropdown">
                <button
                  type="button"
                  class="nav-link dropdown-toggle"
                  data-bs-toggle="dropdown"
                  aria-expanded="false"
                >
                  AI Tools
                </button>

                <ul class="dropdown-menu">
                  <li>
                    <router-link class="dropdown-item" to="/ai/cv-analyzer">
                      CV Analyzer
                    </router-link>
                  </li>

                  <li>
                    <router-link class="dropdown-item" to="/ai/job-match">
                      Job Matcher
                    </router-link>
                  </li>

                  <li>
                    <router-link class="dropdown-item" to="/ai/job-recommendations">
                      Job-Empfehlungen
                    </router-link>
                  </li>

                  <li>
                    <router-link class="dropdown-item" to="/ai/cover-letter">
                      Anschreiben
                    </router-link>
                  </li>
                </ul>
              </div>
            </template>

            <template v-if="isRecruiter">
              <router-link class="nav-link" to="/recruiter/dashboard">
                Dashboard
              </router-link>

              <router-link class="nav-link" to="/recruiter/jobs">
                Jobs
              </router-link>

              <router-link class="nav-link" to="/recruiter/applications">
                Bewerbungen
              </router-link>

              <router-link class="nav-link" to="/recruiter/candidates">
                Kandidaten
              </router-link>
            </template>

            <template v-if="isAdmin">
              <router-link class="nav-link" to="/admin/dashboard">
                Admin
              </router-link>

              <router-link class="nav-link" to="/admin/companies">
                Unternehmen
              </router-link>

              <router-link class="nav-link" to="/admin/users">
                Benutzer
              </router-link>

              <router-link class="nav-link" to="/admin/company-members">
                Recruiter-Zuweisungen
              </router-link>

              <router-link class="nav-link" to="/recruiter/dashboard">
                Recruiter
              </router-link>
            </template>

            <div class="nav-item dropdown ms-xl-3">
              <button
                type="button"
                class="nav-link dropdown-toggle d-flex align-items-center gap-2 fw-semibold"
                data-bs-toggle="dropdown"
                aria-expanded="false"
              >
                <i class="bi bi-person-circle fs-5" aria-hidden="true"></i>
                <span>{{ displayedRole }}</span>
              </button>

              <ul class="dropdown-menu dropdown-menu-end">
                <li v-if="!isAdmin">
                  <router-link class="dropdown-item" to="/profile">
                    <i class="bi bi-person me-2" aria-hidden="true"></i>
                    Profil
                  </router-link>
                </li>

                <li>
                  <router-link class="dropdown-item" to="/account">
                    <i class="bi bi-gear me-2" aria-hidden="true"></i>
                    Kontoeinstellungen
                  </router-link>
                </li>

                <li>
                  <router-link class="dropdown-item" to="/home">
                    <i class="bi bi-house me-2" aria-hidden="true"></i>
                    Startseite
                  </router-link>
                </li>

                <li>
                  <hr class="dropdown-divider" />
                </li>

                <li>
                  <button
                    type="button"
                    class="dropdown-item text-danger"
                    :disabled="logoutLoading"
                    @click="logout"
                  >
                    <span
                      v-if="logoutLoading"
                      class="spinner-border spinner-border-sm me-2"
                      aria-hidden="true"
                    ></span>
                    <i v-else class="bi bi-box-arrow-right me-2" aria-hidden="true"></i>
                    {{ logoutLoading ? "Abmelden..." : "Abmelden" }}
                  </button>
                </li>
              </ul>
            </div>
          </template>
        </div>
      </div>
    </div>
  </nav>
</template>
