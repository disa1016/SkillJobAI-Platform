<script setup>
import { computed, onMounted, ref } from "vue";

import PageHeader from "@/components/shared/PageHeader.vue";
import api from "@/services/api";

const candidates = ref([]);
const searchSkill = ref("");

const loading = ref(false);
const error = ref("");

const hasCandidates = computed(() => candidates.value.length > 0);

const stats = computed(() => [
    {
        label: "Kandidaten",
        value: candidates.value.length,
        textClass: "",
    },
    {
        label: "Bewerbungen",
        value: candidates.value.reduce(
            (sum, candidate) => sum + (candidate.applicationsCount || 0),
            0
        ),
        textClass: "",
    },
    {
        label: "Angenommen",
        value: candidates.value.reduce(
            (sum, candidate) => sum + (candidate.acceptedApplications || 0),
            0
        ),
        textClass: "text-success",
    },
    {
        label: "Abgelehnt",
        value: candidates.value.reduce(
            (sum, candidate) => sum + (candidate.rejectedApplications || 0),
            0
        ),
        textClass: "text-danger",
    },
]);

const formatDate = (date) => {
    if (!date) return "Kein Datum";

    return new Date(date).toLocaleDateString("de-DE");
};

const loadCandidates = async () => {
    loading.value = true;
    error.value = "";

    try {
        const skill = searchSkill.value.trim();

        const url = skill
            ? `/recruiter/candidates?skill=${encodeURIComponent(skill)}`
            : "/recruiter/candidates";

        const { data } = await api.get(url);
        candidates.value = data;
    } catch {
        error.value = "Kandidaten konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const clearSearch = async () => {
    searchSkill.value = "";
    await loadCandidates();
};

onMounted(loadCandidates);
</script>

<template>
    <main class="container py-4">
        <PageHeader title="Kandidatensuche"
            description="Finde Kandidatinnen und Kandidaten anhand ihrer hinterlegten Skills.">
            <template #actions>
                <router-link to="/recruiter/dashboard" class="btn btn-outline-secondary">
                    <i class="bi bi-arrow-left me-2" aria-hidden="true"></i>
                    Zum Dashboard
                </router-link>
            </template>
        </PageHeader>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div class="row g-3 mb-4">
            <div v-for="stat in stats" :key="stat.label" class="col-6 col-lg-3">
                <div class="card border-0 shadow-sm h-100">
                    <div class="card-body">
                        <h6 class="text-body-secondary">{{ stat.label }}</h6>

                        <h3 class="mb-0" :class="stat.textClass">
                            {{ stat.value }}
                        </h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="card border-0 shadow-sm mb-4">
            <div class="card-body">
                <h5>Nach Skill suchen</h5>

                <div class="row g-2 align-items-center">
                    <div class="col-12 col-lg">
                        <input v-model="searchSkill" class="form-control"
                            placeholder="z. B. PostgreSQL, ASP.NET Core, C#" @keyup.enter="loadCandidates" />
                    </div>

                    <div class="col-12 col-sm-auto d-grid">
                        <button type="button" class="btn btn-primary" :disabled="loading" @click="loadCandidates">
                            Suchen
                        </button>
                    </div>

                    <div class="col-12 col-sm-auto d-grid">
                        <button type="button" class="btn btn-outline-secondary" :disabled="loading || !searchSkill"
                            @click="clearSearch">
                            Zurücksetzen
                        </button>
                    </div>
                </div>

                <p v-if="searchSkill" class="text-body-secondary small mt-2 mb-0">
                    Suche nach Skill:
                    <strong>{{ searchSkill }}</strong>
                </p>
            </div>
        </div>

        <div v-if="loading" class="d-flex justify-content-center py-5">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Kandidaten werden geladen...</span>
            </div>
        </div>

        <template v-else>
            <div v-if="!hasCandidates" class="text-center py-5">
                Keine Kandidaten gefunden.
            </div>

            <div v-else class="row g-3">
                <div v-for="candidate in candidates" :key="candidate.id" class="col-12 col-lg-6">
                    <div class="card border-0 shadow-sm h-100">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-start gap-3">
                                <div>
                                    <h5 class="mb-1">
                                        {{ candidate.fullName || "Unbekannter Kandidat" }}
                                    </h5>

                                    <p class="text-body-secondary mb-2">
                                        {{ candidate.email || "Keine E-Mail" }}
                                    </p>
                                </div>

                                <span class="badge text-bg-primary">
                                    {{ candidate.skillsCount || 0 }} Skills
                                </span>
                            </div>

                            <div class="mb-3">
                                <strong>Skills:</strong>

                                <div v-if="candidate.skills?.length" class="mt-2">
                                    <span v-for="skill in candidate.skills" :key="skill"
                                        class="badge text-bg-success me-2 mb-2">
                                        {{ skill }}
                                    </span>
                                </div>

                                <p v-else class="text-body-secondary mt-2 mb-0">
                                    Keine Skills hinterlegt.
                                </p>
                            </div>

                            <div class="row text-center mt-3">
                                <div class="col-4">
                                    <div class="border rounded p-2">
                                        <strong>{{ candidate.applicationsCount || 0 }}</strong>
                                        <div class="text-body-secondary small">Bewerbungen</div>
                                    </div>
                                </div>

                                <div class="col-4">
                                    <div class="border rounded p-2">
                                        <strong class="text-success">
                                            {{ candidate.acceptedApplications || 0 }}
                                        </strong>
                                        <div class="text-body-secondary small">Angenommen</div>
                                    </div>
                                </div>

                                <div class="col-4">
                                    <div class="border rounded p-2">
                                        <strong class="text-danger">
                                            {{ candidate.rejectedApplications || 0 }}
                                        </strong>
                                        <div class="text-body-secondary small">Abgelehnt</div>
                                    </div>
                                </div>
                            </div>

                            <p class="text-body-secondary small mt-3 mb-0">
                                Registriert am:
                                {{ formatDate(candidate.createdAt) }}
                            </p>

                            <router-link :to="`/recruiter/candidates/${candidate.id}`"
                                class="btn btn-outline-primary btn-sm mt-3">
                                Profil ansehen
                            </router-link>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </main>
</template>