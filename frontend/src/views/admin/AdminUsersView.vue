<script setup>
import { computed, onMounted, ref } from "vue";
import api from "../../services/api";

const users = ref([]);
const loading = ref(false);
const error = ref("");
const success = ref("");

const roles = ["Candidate", "Recruiter", "Admin"];

const hasUsers = computed(() => users.value.length > 0);

const formatDate = (date) => {
    if (!date) return "-";

    return new Date(date).toLocaleDateString("de-DE", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
    });
};

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const loadUsers = async () => {
    loading.value = true;
    clearMessages();

    try {
        const { data } = await api.get("/admin/users");
        users.value = data;
    } catch {
        error.value = "Benutzer konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const updateRole = async (user) => {
    clearMessages();

    try {
        await api.put(`/admin/users/${user.id}/role`, {
            role: user.role,
        });

        success.value = "Rolle erfolgreich geändert.";
    } catch {
        error.value = "Fehler beim Aktualisieren der Rolle.";
    }
};

const deleteUser = async (id) => {
    const confirmed = confirm("Möchtest du diesen Benutzer wirklich löschen?");

    if (!confirmed) return;

    clearMessages();

    try {
        await api.delete(`/admin/users/${id}`);
        users.value = users.value.filter((user) => user.id !== id);

        success.value = "Benutzer wurde gelöscht.";
    } catch {
        error.value = "Fehler beim Löschen des Benutzers.";
    }
};

onMounted(loadUsers);
</script>

<template>
    <div class="container py-4">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
            <h2 class="mb-0">User Management</h2>

            <button type="button" class="btn btn-outline-primary btn-sm" :disabled="loading" @click="loadUsers">
                Aktualisieren
            </button>
        </div>

        <div v-if="loading" class="alert alert-info">
            Lade Benutzer...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <div v-if="success" class="alert alert-success">
                {{ success }}
            </div>

            <div class="card shadow-sm">
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-striped align-middle mb-0">
                            <thead>
                                <tr>
                                    <th>ID</th>
                                    <th>Name</th>
                                    <th>E-Mail</th>
                                    <th>Rolle</th>
                                    <th>Erstellt am</th>
                                    <th>Aktionen</th>
                                </tr>
                            </thead>

                            <tbody>
                                <tr v-for="user in users" :key="user.id">
                                    <td>{{ user.id }}</td>
                                    <td>{{ user.fullName || "Ohne Namen" }}</td>
                                    <td>{{ user.email || "Keine E-Mail" }}</td>

                                    <td style="max-width: 220px">
                                        <select v-model="user.role" class="form-select">
                                            <option v-for="role in roles" :key="role" :value="role">
                                                {{ role }}
                                            </option>
                                        </select>
                                    </td>

                                    <td>{{ formatDate(user.createdAt) }}</td>

                                    <td>
                                        <div class="d-flex flex-wrap gap-2">
                                            <button type="button" class="btn btn-primary btn-sm"
                                                @click="updateRole(user)">
                                                Speichern
                                            </button>

                                            <button type="button" class="btn btn-danger btn-sm"
                                                @click="deleteUser(user.id)">
                                                Löschen
                                            </button>
                                        </div>
                                    </td>
                                </tr>

                                <tr v-if="!hasUsers">
                                    <td colspan="6" class="text-center text-muted">
                                        Keine Benutzer gefunden.
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