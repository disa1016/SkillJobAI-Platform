<script setup>
import { ref, onMounted, computed } from "vue";
import api from "../services/api";

const candidates = ref([]);
const searchSkill = ref("");

const loading = ref(false);
const error = ref("");

const totalCandidates = computed(() => candidates.value.length);

const totalApplications = computed(() =>
    candidates.value.reduce((sum, candidate) => sum + candidate.applicationsCount, 0)
);

const totalAccepted = computed(() =>
    candidates.value.reduce((sum, candidate) => sum + candidate.acceptedApplications, 0)
);

const totalRejected = computed(() =>
    candidates.value.reduce((sum, candidate) => sum + candidate.rejectedApplications, 0)
);

const loadCandidates = async () => {
    loading.value = true;
    error.value = "";

    try {
        const url = searchSkill.value
            ? `/recruiter/candidates?skill=${encodeURIComponent(searchSkill.value)}`
            : "/recruiter/candidates";

        const response = await api.get(url);
        candidates.value = response.data;
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
    <div class="container py-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="mb-0">Kandidaten-Suche</h2>

            <router-link to="/recruiter/dashboard" class="btn btn-outline-secondary">
                Zurück zum Dashboard
            </router-link>
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div class="row g-3 mb-4">
            <div class="col-md-3">
                <div class="card shadow-sm border-0">
                    <div class="card-body">
                        <h6 class="text-muted">Kandidaten</h6>
                        <h3>{{ totalCandidates }}</h3>
                    </div>
                </div>
            </div>

            <div class="col-md-3">
                <div class="card shadow-sm border-0">
                    <div class="card-body">
                        <h6 class="text-muted">Bewerbungen</h6>
                        <h3>{{ totalApplications }}</h3>
                    </div>
                </div>
            </div>

            <div class="col-md-3">
                <div class="card shadow-sm border-0">
                    <div class="card-body">
                        <h6 class="text-muted">Accepted</h6>
                        <h3 class="text-success">{{ totalAccepted }}</h3>
                    </div>
                </div>
            </div>

            <div class="col-md-3">
                <div class="card shadow-sm border-0">
                    <div class="card-body">
                        <h6 class="text-muted">Rejected</h6>
                        <h3 class="text-danger">{{ totalRejected }}</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="card shadow-sm mb-4">
            <div class="card-body">
                <h5>Nach Skill suchen</h5>

                <div class="d-flex gap-2">
                    <input v-model="searchSkill" class="form-control" placeholder="z.B. PostgreSQL, ASP.NET Core, C#"
                        @keyup.enter="loadCandidates" />

                    <button class="btn btn-primary" @click="loadCandidates">
                        Suchen
                    </button>

                    <button class="btn btn-outline-secondary" @click="clearSearch">
                        Zurücksetzen
                    </button>
                </div>

                <p v-if="searchSkill" class="text-muted small mt-2 mb-0">
                    Suche nach Skill: <strong>{{ searchSkill }}</strong>
                </p>
            </div>
        </div>

        <div v-if="loading" class="alert alert-info">
            Kandidaten werden geladen...
        </div>

        <div v-if="!loading && candidates.length === 0" class="alert alert-warning">
            Keine Kandidaten gefunden.
        </div>

        <div v-if="candidates.length > 0" class="row g-3">
            <div v-for="candidate in candidates" :key="candidate.id" class="col-md-6">
                <div class="card shadow-sm h-100">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start">
                            <div>
                                <h5 class="mb-1">
                                    {{ candidate.fullName }}
                                </h5>

                                <p class="text-muted mb-2">
                                    {{ candidate.email }}
                                </p>
                            </div>

                            <span class="badge bg-primary">
                                {{ candidate.skillsCount }} Skills
                            </span>
                        </div>

                        <div class="mb-3">
                            <strong>Skills:</strong>

                            <div v-if="candidate.skills?.length" class="mt-2">
                                <span v-for="skill in candidate.skills" :key="skill" class="badge bg-success me-2 mb-2">
                                    {{ skill }}
                                </span>
                            </div>

                            <p v-else class="text-muted mt-2 mb-0">
                                Keine Skills hinterlegt.
                            </p>
                        </div>

                        <div class="row text-center mt-3">
                            <div class="col-4">
                                <div class="border rounded p-2">
                                    <strong>{{ candidate.applicationsCount }}</strong>
                                    <div class="text-muted small">Bewerbungen</div>
                                </div>
                            </div>

                            <div class="col-4">
                                <div class="border rounded p-2">
                                    <strong class="text-success">
                                        {{ candidate.acceptedApplications }}
                                    </strong>
                                    <div class="text-muted small">Accepted</div>
                                </div>
                            </div>

                            <div class="col-4">
                                <div class="border rounded p-2">
                                    <strong class="text-danger">
                                        {{ candidate.rejectedApplications }}
                                    </strong>
                                    <div class="text-muted small">Rejected</div>
                                </div>
                            </div>
                        </div>

                        <p class="text-muted small mt-3 mb-0">
                            Registriert am:
                            {{ new Date(candidate.createdAt).toLocaleDateString("de-DE") }}
                        </p>

                        <router-link :to="`/recruiter/candidates/${candidate.id}`"
                            class="btn btn-outline-primary btn-sm mt-3">
                            Profil ansehen
                        </router-link>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>