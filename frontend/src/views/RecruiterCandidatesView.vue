<script setup>
import { ref, onMounted } from "vue";
import api from "../services/api";

const candidates = ref([]);
const searchSkill = ref("");

const loading = ref(false);
const error = ref("");

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
        <h2 class="mb-4">Kandidaten-Suche</h2>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
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
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>