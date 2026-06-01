<script setup>
import { useRouter } from "vue-router";

const router = useRouter();

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

            <div class="navbar-nav ms-auto">
                <router-link class="nav-link" to="/dashboard">Dashboard</router-link>
                <router-link class="nav-link" to="/courses">Courses</router-link>
                <router-link class="nav-link" to="/jobs">Jobs</router-link>
                <router-link class="nav-link" to="/profile">Profile</router-link>
                <router-link class="nav-link" to="/ai/cv-analyzer">AI Analyzer</router-link>
                <router-link class="nav-link" to="/ai/job-match">Job Matcher</router-link>
                <router-link class="nav-link" to="/ai/job-recommendations">Job Recommendations</router-link>
                <router-link class="nav-link" to="/ai/cover-letter">Cover Letter</router-link>

                <button class="btn btn-light btn-sm ms-3" @click="logout">
                    Logout
                </button>
            </div>
        </div>
    </nav>
</template>