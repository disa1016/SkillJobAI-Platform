<script setup>
import { computed, ref } from "vue";

const user = ref(JSON.parse(localStorage.getItem("user") || "null"));

const dashboardPath = computed(() => {
    if (user.value?.role === "Admin") return "/admin/dashboard";
    if (user.value?.role === "Recruiter") return "/recruiter/dashboard";

    return "/dashboard";
});

const features = [
    {
        icon: "bi-file-earmark-person",
        title: "AI CV Analyse",
        description:
            "Analysiere deinen Lebenslauf mit KI und erkenne deine Stärken.",
    },
    {
        icon: "bi-bar-chart-line",
        title: "Skill Gap",
        description:
            "Vergleiche deine Skills mit Job-Anforderungen und schließe Lücken.",
    },
    {
        icon: "bi-mortarboard",
        title: "Online Learning",
        description:
            "Lerne passende Kurse für deine fehlenden Skills.",
    },
    {
        icon: "bi-briefcase",
        title: "Recruiting",
        description:
            "Recruiter verwalten Jobs und Bewerbungen zentral.",
    },
];
</script>

<template>
    <main class="landing-page">
        <section class="landing-hero">
            <div class="hero-decoration hero-decoration-left"></div>
            <div class="hero-decoration hero-decoration-right"></div>

            <div class="hero-dots hero-dots-left">
                <span v-for="dot in 15" :key="`left-${dot}`"></span>
            </div>

            <div class="hero-dots hero-dots-right">
                <span v-for="dot in 15" :key="`right-${dot}`"></span>
            </div>

            <div class="hero-wave hero-wave-one"></div>
            <div class="hero-wave hero-wave-two"></div>
            <div class="hero-wave hero-wave-three"></div>

            <div class="container hero-container">
                <div class="hero-content">
                    <span class="hero-badge">
                        AI-powered Career Platform
                    </span>

                    <h1 class="hero-title">
                        <span>SkillJob</span> AI
                    </h1>

                    <p class="hero-description">
                        Deine smarte Plattform für Jobs, Skills und persönliches
                        Wachstum.
                        <br class="d-none d-md-block" />
                        Mit KI an deiner Seite erreichst du deine Ziele schneller.
                    </p>

                    <div class="hero-actions">
                        <template v-if="!user">
                            <router-link to="/register" class="btn landing-primary-button">
                                <i class="bi bi-arrow-up-right me-2"></i>
                                Kostenlos starten
                            </router-link>

                            <router-link to="/login" class="btn landing-secondary-button">
                                Login
                            </router-link>
                        </template>

                        <router-link v-else :to="dashboardPath" class="btn landing-primary-button">
                            <i class="bi bi-graph-up-arrow me-2"></i>
                            Zum Dashboard
                        </router-link>
                    </div>
                </div>
            </div>
        </section>

        <section class="features-section">
            <div class="container">
                <div class="row g-4">
                    <div v-for="feature in features" :key="feature.title" class="col-12 col-md-6 col-lg-3">
                        <article class="feature-card">
                            <div class="feature-icon">
                                <i :class="['bi', feature.icon]"></i>
                            </div>

                            <h2 class="feature-title">
                                {{ feature.title }}
                            </h2>

                            <p class="feature-description">
                                {{ feature.description }}
                            </p>

                            <a href="#" class="feature-link">
                                Mehr erfahren
                                <i class="bi bi-arrow-right"></i>
                            </a>
                        </article>
                    </div>
                </div>
            </div>
        </section>

        <section class="career-section">
            <div class="container text-center">
                <span class="career-label">
                    Warum SkillJob AI?
                </span>

                <h2 class="career-title">
                    Von Skill Gap zu Karrierechance
                </h2>

                <p class="career-description">
                    Unsere KI-gestützten Tools helfen dir, deine Fähigkeiten zu
                    verstehen,
                    <br class="d-none d-md-block" />
                    gezielt zu verbessern und die richtigen Chancen zu nutzen.
                </p>

                <router-link v-if="!user" to="/register" class="btn landing-primary-button mt-4">
                    Jetzt starten
                </router-link>

                <router-link v-else :to="dashboardPath" class="btn landing-primary-button mt-4">
                    Dashboard öffnen
                </router-link>
            </div>
        </section>
    </main>
</template>