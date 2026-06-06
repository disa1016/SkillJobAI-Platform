<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();

const user = ref(
    JSON.parse(localStorage.getItem("user") || "null")
);

const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    router.push("/login");
};
</script>

<template>
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
        <div class="container">
            <router-link class="navbar-brand fw-bold" to="/dashboard">
                SkillJob AI
            </router-link>

            <div class="navbar-nav ms-auto align-items-center">
                <router-link class="nav-link" to="/dashboard">
                    Dashboard
                </router-link>

                <router-link class="nav-link" to="/courses">
                    Courses
                </router-link>

                <router-link class="nav-link" to="/jobs">
                    Jobs
                </router-link>

                <router-link class="nav-link" to="/profile">
                    Profile
                </router-link>

                <router-link class="nav-link" to="/ai/cv-analyzer">
                    AI Analyzer
                </router-link>

                <router-link class="nav-link" to="/ai/job-match">
                    Job Matcher
                </router-link>

                <router-link class="nav-link" to="/ai/job-recommendations">
                    Job Recommendations
                </router-link>

                <router-link class="nav-link" to="/ai/cover-letter">
                    Cover Letter
                </router-link>

                <router-link class="nav-link" to="/recruiter/dashboard">
                    Recruiter
                </router-link>

                <router-link class="nav-link" to="/profile/skills">
                    My Skills
                </router-link>

                <router-link class="nav-link" to="/recruiter/applications">
                    Applications
                </router-link>

                <router-link class="nav-link" to="/my-applications">
                    Meine Bewerbungen
                </router-link>

                <router-link class="nav-link" to="/recruiter/jobs">
  Recruiter Jobs
</router-link>
<router-link
  v-if="user?.role === 'Admin'"
  class="nav-link"
  to="/admin/dashboard"
>
  Admin
</router-link>

                <span v-if="user" class="navbar-text text-white mx-3">
                    Rolle: {{ user.role }}
                </span>

                <button class="btn btn-light btn-sm" @click="logout">
                    Logout
                </button>
            </div>
        </div>
    </nav>
</template>