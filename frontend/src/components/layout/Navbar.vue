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
  <nav class="navbar navbar-expand-xl navbar-light skilljob-navbar">
    <div class="container-fluid px-4">
      <router-link
        class="navbar-brand skilljob-brand"
        :to="homePath"
      >
        <span class="brand-mark">V</span>
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

      <div
        id="mainNavbar"
        class="collapse navbar-collapse"
      >
        <div class="navbar-nav ms-auto align-items-xl-center gap-xl-1">
          <template v-if="!user">
            <router-link
              to="/home"
              class="btn navbar-outline-button btn-sm"
            >
              Startseite
            </router-link>
          </template>

          <template v-else>
            <!-- Candidate Navigation -->
            <template v-if="isCandidate">
              <router-link
                class="nav-link"
                to="/dashboard"
              >
                Dashboard
              </router-link>

              <router-link
                class="nav-link"
                to="/career-roadmap"
              >
                Roadmap
              </router-link>

              <router-link
                class="nav-link"
                to="/courses"
              >
                Courses
              </router-link>

              <router-link
                class="nav-link"
                to="/jobs"
              >
                Jobs
              </router-link>

              <router-link
                class="nav-link"
                to="/profile/skills"
              >
                Skills
              </router-link>

              <router-link
                class="nav-link"
                to="/my-applications"
              >
                Bewerbungen
              </router-link>

              <div class="nav-item dropdown">
                <button
                  type="button"
                  class="nav-link dropdown-toggle btn btn-link text-decoration-none"
                  data-bs-toggle="dropdown"
                  aria-expanded="false"
                >
                  AI Tools
                </button>

                <ul class="dropdown-menu">
                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/ai/cv-analyzer"
                    >
                      CV Analyzer
                    </router-link>
                  </li>

                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/ai/job-match"
                    >
                      Job Matcher
                    </router-link>
                  </li>

                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/ai/job-recommendations"
                    >
                      Job Empfehlungen
                    </router-link>
                  </li>

                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/ai/cover-letter"
                    >
                      Cover Letter
                    </router-link>
                  </li>
                </ul>
              </div>

              <router-link
                class="nav-link"
                to="/profile"
              >
                Profil
              </router-link>

              <router-link
                class="nav-link"
                to="/account"
              >
                Kontoeinstellungen
              </router-link>

              <router-link
                class="nav-link"
                to="/home"
              >
                Startseite
              </router-link>

              <button
                type="button"
                class="nav-link btn btn-link text-danger text-decoration-none text-start"
                :disabled="logoutLoading"
                @click="logout"
              >
                {{ logoutLoading ? "Logout..." : "Logout" }}
              </button>

              <span
                class="navbar-text fw-semibold d-flex align-items-center gap-2 ms-xl-2"
              >
                <span class="user-avatar">
                  <i class="bi bi-person"></i>
                </span>

                Candidate
              </span>
            </template>

            <!-- Recruiter Navigation -->
            <template v-if="isRecruiter">
              <router-link
                class="nav-link"
                to="/recruiter/dashboard"
              >
                Dashboard
              </router-link>

              <router-link
                class="nav-link"
                to="/recruiter/jobs"
              >
                Jobs
              </router-link>

              <router-link
                class="nav-link"
                to="/recruiter/applications"
              >
                Bewerbungen
              </router-link>

              <router-link
                class="nav-link"
                to="/recruiter/candidates"
              >
                Kandidaten
              </router-link>

              <div class="nav-item dropdown ms-xl-3">
                <button
                  type="button"
                  class="nav-link dropdown-toggle btn btn-link text-decoration-none fw-semibold user-menu-link"
                  data-bs-toggle="dropdown"
                  aria-expanded="false"
                >
                  <span class="user-avatar">
                    <i class="bi bi-person"></i>
                  </span>

                  {{ user.role }}
                </button>

                <ul class="dropdown-menu dropdown-menu-end">
                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/profile"
                    >
                      Profil
                    </router-link>
                  </li>

                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/account"
                    >
                      Kontoeinstellungen
                    </router-link>
                  </li>

                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/home"
                    >
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
                      {{ logoutLoading ? "Logout..." : "Logout" }}
                    </button>
                  </li>
                </ul>
              </div>
            </template>

            <!-- Admin Navigation -->
            <template v-if="isAdmin">
              <router-link
                class="nav-link"
                to="/admin/dashboard"
              >
                Admin
              </router-link>

              <router-link
                class="nav-link"
                to="/admin/companies"
              >
                Companies
              </router-link>

              <router-link
                class="nav-link"
                to="/admin/users"
              >
                Users
              </router-link>

              <router-link
                class="nav-link"
                to="/admin/company-members"
              >
                Recruiter Assignments
              </router-link>

              <router-link
                class="nav-link"
                to="/recruiter/dashboard"
              >
                Recruiter
              </router-link>

              <div class="nav-item dropdown ms-xl-3">
                <button
                  type="button"
                  class="nav-link dropdown-toggle btn btn-link text-decoration-none fw-semibold user-menu-link"
                  data-bs-toggle="dropdown"
                  aria-expanded="false"
                >
                  <span class="user-avatar">
                    <i class="bi bi-person"></i>
                  </span>

                  {{ user.role }}
                </button>

                <ul class="dropdown-menu dropdown-menu-end">
                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/account"
                    >
                      Kontoeinstellungen
                    </router-link>
                  </li>

                  <li>
                    <router-link
                      class="dropdown-item"
                      to="/home"
                    >
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
                      {{ logoutLoading ? "Logout..." : "Logout" }}
                    </button>
                  </li>
                </ul>
              </div>
            </template>
          </template>
        </div>
      </div>
    </div>
  </nav>
</template>