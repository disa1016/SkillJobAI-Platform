<script setup>
import { computed, onMounted, ref } from "vue";

import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import PageHeader from "@/components/shared/PageHeader.vue";
import { getCompanies } from "@/services/companyService";
import {
    assignCompanyMember,
    getAdminUsers,
    getCompanyMembers,
    removeCompanyMember,
} from "@/services/adminService";

const members = ref([]);
const companies = ref([]);
const users = ref([]);

const selectedCompanyId = ref("");
const selectedUserId = ref("");

const loading = ref(false);
const assigning = ref(false);
const removingMemberId = ref(null);
const error = ref("");
const success = ref("");

const recruiters = computed(() => {
    return users.value.filter((user) =>
        ["Recruiter", "Admin", "Candidate"].includes(user.role)
    );
});

const hasMembers = computed(() => members.value.length > 0);

const formatDate = (date) => {
    if (!date) return "-";
    return new Date(date).toLocaleDateString("de-DE");
};

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const resetForm = () => {
    selectedCompanyId.value = "";
    selectedUserId.value = "";
};

const loadData = async ({ preserveMessages = false } = {}) => {
    loading.value = true;

    if (!preserveMessages) {
        clearMessages();
    }

    try {
        const [membersData, companiesData, usersData] = await Promise.all([
            getCompanyMembers(),
            getCompanies({ page: 1, pageSize: 50 }),
            getAdminUsers(),
        ]);

        members.value = membersData;
        companies.value = companiesData.items;
        users.value = usersData;
    } catch {
        error.value = "Daten konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const assignRecruiter = async () => {
    clearMessages();

    if (!selectedCompanyId.value || !selectedUserId.value) {
        error.value = "Bitte Firma und Recruiter auswählen.";
        return;
    }

    assigning.value = true;

    try {
        await assignCompanyMember(
            Number(selectedCompanyId.value),
            Number(selectedUserId.value)
        );

        resetForm();
        success.value = "Recruiter wurde erfolgreich zugewiesen.";
        await loadData({ preserveMessages: true });
    } catch (err) {
        error.value =
            err.response?.data?.message || "Recruiter konnte nicht zugewiesen werden.";
    } finally {
        assigning.value = false;
    }
};

const removeMember = async (memberId) => {
    if (!confirm("Möchtest du diese Zuweisung wirklich entfernen?")) return;

    clearMessages();
    removingMemberId.value = memberId;

    try {
        await removeCompanyMember(memberId);
        success.value = "Zuweisung wurde entfernt.";
        await loadData({ preserveMessages: true });
    } catch {
        error.value = "Zuweisung konnte nicht entfernt werden.";
    } finally {
        removingMemberId.value = null;
    }
};

onMounted(loadData);
</script>

<template>
    <main class="container py-4">
        <PageHeader title="Recruiter-Firmen-Zuweisung"
            description="Benutzer Firmen zuweisen und bestehende Mitgliedschaften verwalten.">
            <template #actions>
                <router-link to="/admin/dashboard" class="btn btn-outline-secondary">
                    <i class="bi bi-arrow-left me-2" aria-hidden="true"></i>
                    Zum Admin-Dashboard
                </router-link>
            </template>
        </PageHeader>

        <div v-if="error" class="alert alert-danger" role="alert">{{ error }}</div>
        <div v-if="success" class="alert alert-success" role="alert">{{ success }}</div>

        <BaseSpinner v-if="loading" message="Zuweisungen werden geladen..." />

        <template v-else>
            <div class="card border-0 shadow-sm mb-4">
                <div class="card-header bg-body border-bottom">
                    <h2 class="h5 mb-0">Recruiter einer Firma zuweisen</h2>
                </div>
                <div class="card-body">
                    <form class="row g-3 align-items-end" @submit.prevent="assignRecruiter">
                        <div class="col-12 col-lg-5">
                            <label for="companySelect" class="form-label">Firma</label>
                            <select id="companySelect" v-model="selectedCompanyId" class="form-select"
                                :disabled="assigning">
                                <option value="">Firma auswählen</option>
                                <option v-for="company in companies" :key="company.id" :value="company.id">
                                    {{ company.name || "Unbekannte Firma" }} · {{ company.location || "Kein Standort" }}
                                </option>
                            </select>
                        </div>

                        <div class="col-12 col-lg-5">
                            <label for="recruiterSelect" class="form-label">Benutzer</label>
                            <select id="recruiterSelect" v-model="selectedUserId" class="form-select"
                                :disabled="assigning">
                                <option value="">Benutzer auswählen</option>
                                <option v-for="user in recruiters" :key="user.id" :value="user.id">
                                    {{ user.fullName || "Ohne Namen" }} · {{ user.email || "Keine E-Mail" }} ({{
                                    user.role }})
                                </option>
                            </select>
                        </div>

                        <div class="col-12 col-lg-2 d-grid">
                            <button type="submit" class="btn btn-primary" :disabled="assigning">
                                <span v-if="assigning" class="spinner-border spinner-border-sm me-2"
                                    aria-hidden="true"></span>
                                {{ assigning ? "Wird zugewiesen..." : "Zuweisen" }}
                            </button>
                        </div>
                    </form>

                    <div class="alert alert-info mt-3 mb-0" role="note">
                        <i class="bi bi-info-circle me-2" aria-hidden="true"></i>
                        Wird ein Candidate ausgewählt, wird seine Rolle automatisch auf Recruiter geändert.
                    </div>
                </div>
            </div>

            <div class="card border-0 shadow-sm">
                <div class="card-header bg-body border-bottom d-flex justify-content-between align-items-center gap-3">
                    <h2 class="h5 mb-0">Aktuelle Zuweisungen</h2>
                    <span class="badge text-bg-secondary">{{ members.length }}</span>
                </div>
                <div class="card-body p-0">
                    <div v-if="hasMembers" class="table-responsive">
                        <table class="table table-hover align-middle mb-0">
                            <thead>
                                <tr>
                                    <th scope="col">Firma</th>
                                    <th scope="col">Standort</th>
                                    <th scope="col">Recruiter</th>
                                    <th scope="col">E-Mail</th>
                                    <th scope="col">Rolle</th>
                                    <th scope="col">Zugewiesen am</th>
                                    <th scope="col" class="text-end">Aktionen</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="member in members" :key="member.id">
                                    <td class="fw-semibold">{{ member.company?.name || "Keine Firma" }}</td>
                                    <td>{{ member.company?.location || "-" }}</td>
                                    <td>{{ member.recruiter?.fullName || "Unbekannt" }}</td>
                                    <td class="text-break">{{ member.recruiter?.email || "Keine E-Mail" }}</td>
                                    <td><span class="badge text-bg-primary">{{ member.role || "Recruiter" }}</span></td>
                                    <td>{{ formatDate(member.joinedAt) }}</td>
                                    <td class="text-end">
                                        <button type="button" class="btn btn-outline-danger btn-sm"
                                            :disabled="removingMemberId === member.id" @click="removeMember(member.id)">
                                            <span v-if="removingMemberId === member.id"
                                                class="spinner-border spinner-border-sm me-2"></span>
                                            Entfernen
                                        </button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <BaseEmptyState v-else icon="bi-people" title="Keine Zuweisungen vorhanden"
                        message="Aktuell ist kein Recruiter einer Firma zugewiesen." />
                </div>
            </div>
        </template>
    </main>
</template>
