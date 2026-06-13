<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const users = ref([]);
const loading = ref(false);
const error = ref("");
const success = ref("");

const formatDate = (date) => {
    if (!date) return "-";

    return new Date(date).toLocaleDateString("de-DE", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
    });
};

const loadUsers = async () => {
    loading.value = true;
    error.value = "";

    try {
        const response = await api.get("/admin/users");
        users.value = response.data;
    } catch {
        error.value = "Benutzer konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const updateRole = async (user) => {
    error.value = "";
    success.value = "";

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
    const confirmed = confirm(
        "Möchtest du diesen Benutzer wirklich löschen?"
    );

    if (!confirmed) return;

    error.value = "";
    success.value = "";

    try {
        await api.delete(`/admin/users/${id}`);

        users.value = users.value.filter(
            (u) => u.id !== id
        );

        success.value = "Benutzer wurde gelöscht.";
    } catch {
        error.value = "Fehler beim Löschen des Benutzers.";
    }
};

onMounted(loadUsers);
</script>

<template>
    <div class="container py-4">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2>User Management</h2>

            <button class="btn btn-outline-primary btn-sm" @click="loadUsers">
                Aktualisieren
            </button>
        </div>

        <div v-if="loading" class="alert alert-info">
            Lade Benutzer...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div v-if="!loading" class="card shadow-sm">
            <div class="card-body">
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
                            <td>{{ user.fullName }}</td>
                            <td>{{ user.email }}</td>

                            <td style="max-width: 220px;">
                                <select v-model="user.role" class="form-select">
                                    <option value="Candidate">Candidate</option>
                                    <option value="Recruiter">Recruiter</option>
                                    <option value="Admin">Admin</option>
                                </select>
                            </td>

                            <td>{{ formatDate(user.createdAt) }}</td>

                            <td>
                                <button class="btn btn-primary btn-sm me-2" @click="updateRole(user)">
                                    Speichern
                                </button>

                                <button class="btn btn-danger btn-sm" @click="deleteUser(user.id)">
                                    Löschen
                                </button>
                            </td>
                        </tr>

                        <tr v-if="users.length === 0">
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