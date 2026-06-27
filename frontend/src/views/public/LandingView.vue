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
    title: "AI CV Analyse",
    description: "Analysiere deinen Lebenslauf und erkenne deine Stärken.",
  },
  {
    title: "Skill Gap",
    description: "Vergleiche deine Skills mit Job-Anforderungen.",
  },
  {
    title: "Online Learning",
    description: "Lerne passende Kurse für deine fehlenden Skills.",
  },
  {
    title: "Recruiting",
    description: "Recruiter verwalten Jobs und Bewerbungen zentral.",
  },
];
</script>

<template>
  <div>
    <section class="bg-primary text-white py-5">
      <div class="container py-5 text-center">
        <h1 class="display-4 fw-bold">SkillJob AI</h1>

        <p class="lead mt-3">
          AI-powered Career Platform für Jobs, Skills und Lernen.
        </p>

        <div class="mt-4">
          <template v-if="!user">
            <router-link to="/register" class="btn btn-light btn-lg me-2">
              Kostenlos starten
            </router-link>

            <router-link to="/login" class="btn btn-outline-light btn-lg">
              Login
            </router-link>
          </template>

          <router-link
            v-else
            :to="dashboardPath"
            class="btn btn-light btn-lg"
          >
            Zum Dashboard
          </router-link>
        </div>
      </div>
    </section>

    <section class="container py-5">
      <div class="row g-4 text-center">
        <div
          v-for="feature in features"
          :key="feature.title"
          class="col-md-3"
        >
          <div class="card h-100 shadow-sm">
            <div class="card-body">
              <h5>{{ feature.title }}</h5>

              <p class="text-muted">
                {{ feature.description }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="bg-light py-5">
      <div class="container text-center">
        <h2>Von Skill Gap zu Karrierechance</h2>

        <p class="text-muted mt-3">
          SkillJob AI verbindet Jobs, Lernen, Bewerbungen und KI in einer Plattform.
        </p>

        <router-link
          v-if="!user"
          to="/register"
          class="btn btn-primary btn-lg mt-3"
        >
          Jetzt starten
        </router-link>

        <router-link
          v-else
          :to="dashboardPath"
          class="btn btn-primary btn-lg mt-3"
        >
          Dashboard öffnen
        </router-link>
      </div>
    </section>
  </div>
</template>