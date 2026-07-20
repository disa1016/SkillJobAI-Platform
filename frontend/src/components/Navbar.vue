<script setup>
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import {
  logout as logoutUser,
} from "@/services/authService";

const router = useRouter();

const user = ref(
  JSON.parse(
    localStorage.getItem("user") || "null"
  )
);

const logoutLoading = ref(false);

const isAdmin = computed(
  () => user.value?.role === "Admin"
);

const isRecruiter = computed(
  () => user.value?.role === "Recruiter"
);

const isCandidate = computed(
  () =>
    user.value?.role === "Candidate" ||
    user.value?.role === "Student"
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

const logout = async () => {
  if (logoutLoading.value) {
    return;
  }

  logoutLoading.value = true;

  try {
    await logoutUser();

    user.value = null;

    await router.replace("/login");
  } finally {
    logoutLoading.value = false;
  }
};
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
    <div class="container-fluid px-4">
      <router-link class="navbar-brand fw-bold" :to="homePath">
        SkillJob AI
      </router-link>

      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#mainNavbar"
        aria-controls="mainNavbar" aria-expanded="false" aria-label="Navigation öffnen">
        <span class="navbar-toggler-icon"></span>
      </button>

      <div id="mainNavbar" class="collapse navbar-collapse">
        <div class="navbar-nav ms-auto align-items-lg-center gap-lg-1">
          <template v-if="!user">
            <router-link to="/home" class="btn btn-outline-light btn-sm">
              Startseite
            </router-link>
          </template>

          <template v-else>
            <!-- Candidate Navigation -->
            <template v-if="isCandidate">
              <router-link class="nav-link" to="/dashboard">
                Dashboard
              </router-link>

              <router-link class="nav-link" to="/career-roadmap">
                Roadmap
              </router-link>

              <router-link class="nav-link" to="/courses">
                Courses
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
                <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown"
                  aria-expanded="false">
                  AI Tools
                </a>

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
                      Job Empfehlungen
                    </router-link>
                  </li>

                  <li>
                    <router-link class="dropdown-item" to="/ai/cover-letter">
                      Cover Letter
                    </router-link>
                  </li>
                </ul>
              </div>
            </template>

            <!-- Recruiter Navigation -->
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

            <!-- Admin Navigation -->
            <template v-if="isAdmin">
              <router-link class="nav-link" to="/admin/dashboard">
                Admin
              </router-link>

              <router-link class="nav-link" to="/admin/companies">
                Companies
              </router-link>

              <router-link class="nav-link" to="/admin/users">
                Users
              </router-link>

              <router-link class="nav-link" to="/admin/company-members">
                Recruiter Assignments
              </router-link>

              <router-link class="nav-link" to="/recruiter/dashboard">
                Recruiter
              </router-link>
            </template>

            <!-- User Menu -->
            <div class="nav-item dropdown ms-lg-3">
              <a class="nav-link dropdown-toggle fw-semibold text-white" href="#" role="button"
                data-bs-toggle="dropdown" aria-expanded="false">
                {{ user.fullName }} · {{ user.role }}
              </a>

              <ul class="dropdown-menu dropdown-menu-end">
                <li>
                  <router-link class="dropdown-item" to="/profile">
                    Profil
                  </router-link>
                </li>

                <li>
                  <router-link class="dropdown-item" to="/home">
                    Startseite
                  </router-link>
                </li>

                <li>
                  <hr class="dropdown-divider" />
                </li>

                <li>
                  <button class="dropdown-item text-danger" type="button" :disabled="logoutLoading" @click="logout">
                    {{
                      logoutLoading
                        ? "Abmeldung..."
                    : "Logout"
                    }}
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