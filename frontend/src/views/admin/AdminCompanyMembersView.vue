<script setup>
import { computed, onMounted, ref } from "vue";

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

const loadData = async () => {
  loading.value = true;
  clearMessages();

  try {
    const [membersData, companiesData, usersData] = await Promise.all([
      getCompanyMembers(),
      getCompanies({
        page: 1,
        pageSize: 50,
      }),
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

    await loadData();
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

  try {
    await removeCompanyMember(memberId);
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
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
            <h2 class="mb-0">Recruiter Firmen-Zuweisung</h2>

            <router-link to="/admin/dashboard" class="btn btn-outline-secondary">
                Zurück zum Admin Dashboard
            </router-link>
        </div>

        <div v-if="loading" class="alert alert-info">
            Daten werden geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
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
                                    {{ company.name || "Unbekannte Firma" }} -
                                    {{ company.location || "Kein Standort" }}
                                </option>
                            </select>
                        </div>

                        <div class="col-md-5">
                            <select v-model="selectedUserId" class="form-select">
                                <option value="">Recruiter auswählen</option>

                                <option v-for="user in recruiters" :key="user.id" :value="user.id">
                                    {{ user.fullName || "Ohne Namen" }} -
                                    {{ user.email || "Keine E-Mail" }}
                                    ({{ user.role }})
                                </option>
                            </select>
                        </div>

                        <div class="col-md-2 d-grid">
                            <button type="button" class="btn btn-primary" :disabled="assigning"
                                @click="assignRecruiter">
                                {{ assigning ? "Wird zugewiesen..." : "Zuweisen" }}
                            </button>
                        </div>
                    </div>

                    <p class="text-muted small mt-2 mb-0">
                        Hinweis: Wenn ein Candidate ausgewählt wird, wird seine Rolle automatisch auf Recruiter
                        geändert.
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
                                    <td>{{ member.company?.name || "Keine Firma" }}</td>
                                    <td>{{ member.company?.location || "-" }}</td>
                                    <td>{{ member.recruiter?.fullName || "Unbekannt" }}</td>
                                    <td>{{ member.recruiter?.email || "Keine E-Mail" }}</td>

                                    <td>
                                        <span class="badge bg-primary">
                                            {{ member.role || "Recruiter" }}
                                        </span>
                                    </td>

                                    <td>{{ formatDate(member.joinedAt) }}</td>

                                    <td>
                                        <button type="button" class="btn btn-danger btn-sm"
                                            @click="removeMember(member.id)">
                                            Entfernen
                                        </button>
                                    </td>
                                </tr>

                                <tr v-if="!hasMembers">
                                    <td colspan="8" class="text-center text-muted">
                                        Keine Zuweisungen vorhanden.
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>