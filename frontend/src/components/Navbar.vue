<script setup>
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();

const user = ref(JSON.parse(localStorage.getItem("user") || "null"));

const isAdmin = computed(() => user.value?.role === "Admin");
const isRecruiter = computed(() => user.value?.role === "Recruiter");
const isCandidate = computed(
  () => user.value?.role === "Student" || user.value?.role === "Candidate"
);

const logout = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
  user.value = null;
  router.push("/login");
};
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
    <div class="container-fluid px-4">
      <router-link class="navbar-brand fw-bold"
        :to="user ? (isAdmin ? '/admin/dashboard' : isRecruiter ? '/recruiter/dashboard' : '/dashboard') : '/home'">
        SkillJob AI
      </router-link>

      <div class="navbar-nav ms-auto align-items-center">
        <template v-if="!user">
          <router-link to="/home" class="btn btn-outline-light btn-sm">
            Startseite
          </router-link>
        </template>

        <template v-else>
          <template v-if="isCandidate">
            <router-link class="nav-link" to="/dashboard">Dashboard</router-link>
            <router-link class="nav-link" to="/courses">Courses</router-link>
            <router-link class="nav-link" to="/jobs">Jobs</router-link>
            <router-link class="nav-link" to="/profile/skills">My Skills</router-link>
            <router-link class="nav-link" to="/my-applications">Meine Bewerbungen</router-link>
            <router-link class="nav-link" to="/profile">Profile</router-link>
            <router-link class="nav-link" to="/ai/cv-analyzer">AI Analyzer</router-link>
            <router-link class="nav-link" to="/ai/job-match">Job Matcher</router-link>
            <router-link class="nav-link" to="/ai/job-recommendations">Job Recommendations</router-link>
            <router-link class="nav-link" to="/ai/cover-letter">Cover Letter</router-link>
          </template>

          <template v-if="isRecruiter">
            <router-link class="nav-link" to="/recruiter/dashboard">Recruiter Dashboard</router-link>
            <router-link class="nav-link" to="/recruiter/jobs">Jobs verwalten</router-link>
            <router-link class="nav-link" to="/recruiter/applications">Bewerbungen</router-link>
            <router-link class="nav-link" to="/profile">Profile</router-link>
          </template>

          <template v-if="isAdmin">
            <router-link class="nav-link" to="/admin/dashboard">Admin Dashboard</router-link>
            <router-link class="nav-link" to="/admin/users">User Management</router-link>
            <router-link class="nav-link" to="/recruiter/dashboard">Recruiter Dashboard</router-link>
            <router-link class="nav-link" to="/recruiter/jobs">Jobs verwalten</router-link>
            <router-link class="nav-link" to="/recruiter/applications">Bewerbungen</router-link>
            <router-link class="nav-link" to="/jobs">Alle Jobs</router-link>
            <router-link class="nav-link" to="/courses">Courses</router-link>
            <router-link class="nav-link" to="/profile">Profile</router-link>
          </template>

          <span class="navbar-text text-white mx-3">
            {{ user.fullName }} · {{ user.role }}
          </span>

          <router-link to="/home" class="btn btn-outline-light btn-sm me-2">
            Startseite
          </router-link>

          <button class="btn btn-light btn-sm" @click="logout">
            Logout
          </button>
        </template>
      </div>
    </div>
  </nav>
</template>