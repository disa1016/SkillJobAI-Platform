<script setup>
import { computed, onMounted, ref } from "vue";

import PageHeader from "@/components/shared/PageHeader.vue";

import {
    deleteAdminUser,
    getAdminUsers,
    updateUserRole,
} from "@/services/adminService";

import { formatDate } from "@/utils/date";

const users = ref([]);
const loading = ref(false);
const error = ref("");
const success = ref("");

const roles = ["Candidate", "Recruiter", "Admin"];

const hasUsers = computed(() => users.value.length > 0);

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const loadUsers = async () => {
    loading.value = true;
    clearMessages();

    try {
        users.value = await getAdminUsers();
    } catch {
        error.value = "Benutzer konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const updateRole = async (user) => {
    clearMessages();

    try {
        await updateUserRole(user.id, user.role);
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
        await deleteAdminUser(id);
        users.value = users.value.filter((user) => user.id !== id);

        success.value = "Benutzer wurde gelöscht.";
    } catch {
        error.value = "Fehler beim Löschen des Benutzers.";
    }
};

onMounted(loadUsers);
</script>


<template>
    <main class="container py-4">
        <PageHeader title="Benutzerverwaltung" description="Rollen verwalten und Benutzerkonten administrieren.">
            <template #actions>
                <button type="button" class="btn btn-outline-primary" :disabled="loading" @click="loadUsers">
                    <i class="bi bi-arrow-clockwise me-2" aria-hidden="true"></i>
                    Aktualisieren
                </button>
            </template>
        </PageHeader>

        <div v-if="loading" class="d-flex justify-content-center py-5">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Benutzer werden geladen...</span>
            </div>
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <div v-if="success" class="alert alert-success">
                {{ success }}
            </div>

            <div class="card border-0 shadow-sm">
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-hover align-middle mb-0">
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

                                    <td>
                                        <select v-model="user.role" class="form-select form-select-sm">
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

                                            <button type="button" class="btn btn-outline-danger btn-sm"
                                                @click="deleteUser(user.id)">
                                                Löschen
                                            </button>
                                        </div>
                                    </td>
                                </tr>

                                <tr v-if="!hasUsers">
                                    <td colspan="6" class="text-center text-body-secondary py-5">
                                        Keine Benutzer gefunden.
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </template>
    </main>
</template>