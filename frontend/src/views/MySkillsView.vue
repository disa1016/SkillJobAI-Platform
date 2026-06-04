<script setup>
import { onMounted, ref } from "vue";
import api from "../services/api";

const allSkills = ref([]);
const mySkills = ref([]);
const selectedSkillId = ref("");
const loading = ref(true);
const error = ref("");
const success = ref("");

const loadData = async () => {
  try {
    const skillsResponse = await api.get("/skills");
    const mySkillsResponse = await api.get("/users/skills/my");

    allSkills.value = skillsResponse.data;
    mySkills.value = mySkillsResponse.data;
  } catch {
    error.value = "Skills konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};

const addSkill = async () => {
  if (!selectedSkillId.value) return;

  error.value = "";
  success.value = "";

  try {
    await api.post(`/users/skills/${selectedSkillId.value}`);
    success.value = "Skill wurde hinzugefügt.";
    selectedSkillId.value = "";
    await loadData();
  } catch (err) {
    error.value = err.response?.data?.message || "Skill konnte nicht hinzugefügt werden.";
  }
};

const removeSkill = async (skillId) => {
  error.value = "";
  success.value = "";

  try {
    await api.delete(`/users/skills/${skillId}`);
    success.value = "Skill wurde entfernt.";
    await loadData();
  } catch {
    error.value = "Skill konnte nicht entfernt werden.";
  }
};

onMounted(loadData);
</script>

<template>
  <div class="container py-4">
    <h2 class="mb-4">Meine Skills</h2>

    <div v-if="loading" class="alert alert-info">
      Skills werden geladen...
    </div>

    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-if="success" class="alert alert-success">
      {{ success }}
    </div>

    <div class="card shadow-sm mb-4">
      <div class="card-body">
        <h5>Neuen Skill hinzufügen</h5>

        <div class="d-flex gap-2">
          <select v-model="selectedSkillId" class="form-select">
            <option value="">Skill auswählen</option>
            <option
              v-for="skill in allSkills"
              :key="skill.id"
              :value="skill.id"
            >
              {{ skill.name }}
            </option>
          </select>

          <button class="btn btn-primary" @click="addSkill">
            Hinzufügen
          </button>
        </div>
      </div>
    </div>

    <div class="card shadow-sm">
      <div class="card-body">
        <h5>Aktuelle Skills</h5>

        <ul v-if="mySkills.length > 0" class="list-group">
          <li
            v-for="skill in mySkills"
            :key="skill.id"
            class="list-group-item d-flex justify-content-between align-items-center"
          >
            <span>✅ {{ skill.name }}</span>

            <button
              class="btn btn-sm btn-outline-danger"
              @click="removeSkill(skill.id)"
            >
              Entfernen
            </button>
          </li>
        </ul>

        <p v-else class="text-muted">
          Du hast noch keine Skills hinzugefügt.
        </p>
      </div>
    </div>
  </div>
</template>