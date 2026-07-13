<script setup>
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

import { USER_ROLES } from "@/constants/roles";
import { clearAuthStorage, getCurrentUser } from "@/utils/storage";

const router = useRouter();

const user = ref(getCurrentUser());

const isAdmin = computed(() => user.value?.role === USER_ROLES.ADMIN);
const isRecruiter = computed(() => user.value?.role === USER_ROLES.RECRUITER);
const isCandidate = computed(
  () =>
    user.value?.role === USER_ROLES.CANDIDATE ||
    user.value?.role === USER_ROLES.STUDENT
);

const homePath = computed(() => {
  if (!user.value) return "/home";
  if (isAdmin.value) return "/admin/dashboard";
  if (isRecruiter.value) return "/recruiter/dashboard";

  return "/dashboard";
});

const logout = () => {
  clearAuthStorage();

  user.value = null;

  router.push("/login");
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
                  <button class="dropdown-item text-danger" @click="logout">
                    Logout
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