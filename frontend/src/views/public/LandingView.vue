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
    <main>
        <section class="bg-body-tertiary border-bottom py-5">
            <div class="container py-4 py-lg-5">
                <div class="row justify-content-center text-center">
                    <div class="col-12 col-lg-9 col-xl-8">
                        <span class="badge text-bg-primary rounded-pill px-3 py-2 mb-3">
                            AI-powered Career Platform
                        </span>

                        <h1 class="display-3 fw-bold mb-3">
                            SkillJob <span class="text-primary">AI</span>
                        </h1>

                        <p class="lead text-body-secondary mb-4">
                            Deine smarte Plattform für Jobs, Skills und persönliches Wachstum.
                            <br class="d-none d-md-block" />
                            Mit KI an deiner Seite erreichst du deine Ziele schneller.
                        </p>

                        <div class="d-flex flex-column flex-sm-row justify-content-center gap-2">
                            <template v-if="!user">
                                <router-link to="/register" class="btn btn-primary btn-lg">
                                    <i class="bi bi-arrow-up-right me-2" aria-hidden="true"></i>
                                    Kostenlos starten
                                </router-link>

                                <router-link to="/login" class="btn btn-outline-primary btn-lg">
                                    <i class="bi bi-box-arrow-in-right me-2" aria-hidden="true"></i>
                                    Anmelden
                                </router-link>
                            </template>

                            <router-link v-else :to="dashboardPath" class="btn btn-primary btn-lg">
                                <i class="bi bi-graph-up-arrow me-2" aria-hidden="true"></i>
                                Zum Dashboard
                            </router-link>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <section class="py-5">
            <div class="container">
                <div class="text-center mb-4">
                    <h2 class="h3 mb-2">Alles für deine berufliche Entwicklung</h2>
                    <p class="text-body-secondary mb-0">
                        Entdecke zentrale Funktionen für Karriere, Lernen und Recruiting.
                    </p>
                </div>

                <div class="row g-4">
                    <div v-for="feature in features" :key="feature.title" class="col-12 col-md-6 col-xl-3">
                        <article class="card border-0 shadow-sm h-100">
                            <div class="card-body p-4">
                                <div
                                    class="d-inline-flex align-items-center justify-content-center bg-primary-subtle text-primary rounded-3 p-3 mb-3">
                                    <i :class="['bi', feature.icon, 'fs-3']" aria-hidden="true"></i>
                                </div>

                                <h3 class="h5 card-title mb-2">
                                    {{ feature.title }}
                                </h3>

                                <p class="card-text text-body-secondary mb-0">
                                    {{ feature.description }}
                                </p>
                            </div>
                        </article>
                    </div>
                </div>
            </div>
        </section>

        <section class="bg-body-tertiary border-top py-5">
            <div class="container py-3">
                <div class="row justify-content-center text-center">
                    <div class="col-12 col-lg-8">
                        <span class="badge text-bg-secondary mb-3">
                            Warum SkillJob AI?
                        </span>

                        <h2 class="display-6 fw-semibold mb-3">
                            Von Skill Gap zu Karrierechance
                        </h2>

                        <p class="lead text-body-secondary mb-4">
                            Unsere KI-gestützten Tools helfen dir, deine Fähigkeiten zu verstehen,
                            gezielt zu verbessern und die richtigen Chancen zu nutzen.
                        </p>

                        <router-link v-if="!user" to="/register" class="btn btn-primary btn-lg">
                            Jetzt starten
                        </router-link>

                        <router-link v-else :to="dashboardPath" class="btn btn-primary btn-lg">
                            Dashboard öffnen
                        </router-link>
                    </div>
                </div>
            </div>
        </section>
    </main>
</template>
