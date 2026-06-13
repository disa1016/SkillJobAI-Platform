<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const users = ref([]);
const loading = ref(false);

const loadUsers = async () => {
    loading.value = true;

    try {
        const response = await api.get("/admin/users");
        users.value = response.data;
    } finally {
        loading.value = false;
    }
};

const updateRole = async (user) => {
    try {
        await api.put(`/admin/users/${user.id}/role`, {
            role: user.role,
        });

        alert("Rolle erfolgreich geändert.");
    } catch {
        alert("Fehler beim Aktualisieren.");
    }
};

const deleteUser = async (id) => {
  const confirmed = confirm(
    "Möchtest du diesen Benutzer wirklich löschen?"
  );

  if (!confirmed) return;

  try {
    await api.delete(`/admin/users/${id}`);

    users.value = users.value.filter(
      (u) => u.id !== id
    );

    alert("Benutzer gelöscht.");
  } catch {
    alert("Fehler beim Löschen.");
  }
};

onMounted(loadUsers);
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">User Management</h2>

        <div v-if="loading" class="alert alert-info">
            Lade Benutzer...
        </div>

        <table v-else class="table table-striped">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>E-Mail</th>
                    <th>Rolle</th>
                    <th>Aktionen</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="user in users" :key="user.id">
                    <td>{{ user.id }}</td>
                    <td>{{ user.fullName }}</td>
                    <td>{{ user.email }}</td>

                    <td>
                        <select v-model="user.role" class="form-select">
                            <option value="Candidate">Candidate</option>
                            <option value="Recruiter">Recruiter</option>
                            <option value="Admin">Admin</option>
                        </select>
                    </td>

                    <td>
                        <button class="btn btn-primary btn-sm me-2" @click="updateRole(user)">
                            Speichern
                        </button>

                        <button class="btn btn-danger btn-sm" @click="deleteUser(user.id)">
                            Löschen
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>