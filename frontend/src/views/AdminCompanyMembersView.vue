<script setup>
import { onMounted, ref, computed } from "vue";
import api from "../services/api";

const members = ref([]);
const companies = ref([]);
const users = ref([]);

const selectedCompanyId = ref("");
const selectedUserId = ref("");

const loading = ref(false);
const error = ref("");
const success = ref("");

const recruiters = computed(() =>
    users.value.filter(
        (user) => user.role === "Recruiter" || user.role === "Admin" || user.role === "Candidate"
    )
);

const loadData = async () => {
    loading.value = true;
    error.value = "";

    try {
        const [membersResponse, companiesResponse, usersResponse] =
            await Promise.all([
                api.get("/company-members"),
                api.get("/companies"),
                api.get("/admin/users"),
            ]);

        members.value = membersResponse.data;
        companies.value = companiesResponse.data;
        users.value = usersResponse.data;
    } catch {
        error.value = "Daten konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const assignRecruiter = async () => {
    error.value = "";
    success.value = "";

    if (!selectedCompanyId.value || !selectedUserId.value) {
        error.value = "Bitte Firma und Recruiter auswählen.";
        return;
    }

    try {
        await api.post("/company-members", {
            companyId: Number(selectedCompanyId.value),
            userId: Number(selectedUserId.value),
            role: "Recruiter",
        });

        selectedCompanyId.value = "";
        selectedUserId.value = "";
        success.value = "Recruiter wurde erfolgreich zugewiesen.";

        await loadData();
    } catch (err) {
        error.value =
            err.response?.data?.message || "Recruiter konnte nicht zugewiesen werden.";
    }
};

const removeMember = async (memberId) => {
    if (!confirm("Möchtest du diese Zuweisung wirklich entfernen?")) return;

    error.value = "";
    success.value = "";

    try {
        await api.delete(`/company-members/${memberId}`);
        success.value = "Zuweisung wurde entfernt.";
        await loadData();
    } catch {
        error.value = "Zuweisung konnte nicht entfernt werden.";
    }
};

onMounted(loadData);
</script>

<template>
    <div class="container py-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="mb-0">Recruiter Firmen-Zuweisung</h2>

            <router-link to="/admin/dashboard" class="btn btn-outline-secondary">
                Zurück zum Admin Dashboard
            </router-link>
        </div>

        <div v-if="loading" class="alert alert-info">
            Daten werden geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div class="card shadow-sm mb-4">
            <div class="card-body">
                <h5 class="mb-3">Recruiter einer Firma zuweisen</h5>

                <div class="row g-2">
                    <div class="col-md-5">
                        <select v-model="selectedCompanyId" class="form-select">
                            <option value="">Firma auswählen</option>
                            <option v-for="company in companies" :key="company.id" :value="company.id">
                                {{ company.name }} - {{ company.location || "Kein Standort" }}
                            </option>
                        </select>
                    </div>

                    <div class="col-md-5">
                        <select v-model="selectedUserId" class="form-select">
                            <option value="">Recruiter auswählen</option>
                            <option v-for="user in recruiters" :key="user.id" :value="user.id">
                                {{ user.fullName }} - {{ user.email }} ({{ user.role }})
                            </option>
                        </select>
                    </div>

                    <div class="col-md-2 d-grid">
                        <button class="btn btn-primary" @click="assignRecruiter">
                            Zuweisen
                        </button>
                    </div>
                </div>

                <p class="text-muted small mt-2 mb-0">
                    Hinweis: Wenn ein Candidate ausgewählt wird, wird seine Rolle automatisch auf Recruiter geändert.
                </p>
            </div>
        </div>

        <div class="card shadow-sm">
            <div class="card-body">
                <h5 class="mb-3">Aktuelle Zuweisungen</h5>

                <div class="table-responsive">
                    <table class="table table-striped align-middle mb-0">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Firma</th>
                                <th>Standort</th>
                                <th>Recruiter</th>
                                <th>E-Mail</th>
                                <th>Rolle</th>
                                <th>Zugewiesen am</th>
                                <th>Aktionen</th>
                            </tr>
                        </thead>

                        <tbody>
                            <tr v-for="member in members" :key="member.id">
                                <td>{{ member.id }}</td>
                                <td>{{ member.company?.name }}</td>
                                <td>{{ member.company?.location || "-" }}</td>
                                <td>{{ member.recruiter?.fullName }}</td>
                                <td>{{ member.recruiter?.email }}</td>
                                <td>
                                    <span class="badge bg-primary">
                                        {{ member.role }}
                                    </span>
                                </td>
                                <td>
                                    {{ new Date(member.joinedAt).toLocaleDateString("de-DE") }}
                                </td>
                                <td>
                                    <button class="btn btn-danger btn-sm" @click="removeMember(member.id)">
                                        Entfernen
                                    </button>
                                </td>
                            </tr>

                            <tr v-if="members.length === 0">
                                <td colspan="8" class="text-center text-muted">
                                    Keine Zuweisungen vorhanden.
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</template>