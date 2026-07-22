<script setup>
import { computed, onMounted, ref } from "vue";
import api from "../../services/api";
import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

const allSkills = ref([]);
const mySkills = ref([]);
const selectedSkillId = ref("");

const loading = ref(true);
const updating = ref(false);
const error = ref("");
const success = ref("");

const hasMySkills = computed(() => mySkills.value.length > 0);

const availableSkills = computed(() => {
    const mySkillIds = mySkills.value.map((skill) => skill.id);

    return allSkills.value.filter((skill) => !mySkillIds.includes(skill.id));
});

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const loadData = async () => {
    loading.value = true;
    clearMessages();

    try {
        const [skillsResponse, mySkillsResponse] = await Promise.all([
            api.get("/skills"),
            api.get("/users/skills/my"),
        ]);

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

    updating.value = true;
    clearMessages();

    try {
        await api.post(`/users/skills/${selectedSkillId.value}`);

        success.value = "Skill wurde hinzugefügt.";
        selectedSkillId.value = "";

        await loadData();
    } catch (err) {
        error.value =
            err.response?.data?.message || "Skill konnte nicht hinzugefügt werden.";
    } finally {
        updating.value = false;
    }
};

const removeSkill = async (skillId) => {
    updating.value = true;
    clearMessages();

    try {
        await api.delete(`/users/skills/${skillId}`);

        success.value = "Skill wurde entfernt.";
        await loadData();
    } catch {
        error.value = "Skill konnte nicht entfernt werden.";
    } finally {
        updating.value = false;
    }
};

onMounted(loadData);
</script>

<template>
  <main class="container py-4">
    <PageHeader
      title="Meine Skills"
      description="Verwalte die Fähigkeiten, die für dein Profil und deine Job-Empfehlungen verwendet werden."
    />

    <BaseSpinner v-if="loading" message="Skills werden geladen..." />

    <template v-else>
      <BaseAlert v-if="error" type="danger" :message="error" />
      <BaseAlert v-if="success" type="success" :message="success" />

      <div class="card border-0 shadow-sm mb-4">
        <div class="card-header bg-body border-bottom">
          <h2 class="h5 mb-0">Neuen Skill hinzufügen</h2>
        </div>
        <div class="card-body">
          <div class="row g-3 align-items-end">
            <div class="col-12 col-md-8">
              <label for="skill-select" class="form-label">Skill</label>
              <select id="skill-select" v-model="selectedSkillId" class="form-select" :disabled="updating">
                <option value="">Skill auswählen</option>
                <option v-for="skill in availableSkills" :key="skill.id" :value="skill.id">
                  {{ skill.name }}
                </option>
              </select>
            </div>
            <div class="col-12 col-md-4 d-grid">
              <button type="button" class="btn btn-primary" :disabled="!selectedSkillId || updating" @click="addSkill">
                <span v-if="updating" class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>
                {{ updating ? "Wird gespeichert..." : "Hinzufügen" }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="card border-0 shadow-sm">
        <div class="card-header bg-body border-bottom">
          <h2 class="h5 mb-0">Aktuelle Skills</h2>
        </div>
        <div class="card-body p-0">
          <ul v-if="hasMySkills" class="list-group list-group-flush">
            <li v-for="skill in mySkills" :key="skill.id" class="list-group-item d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-2 py-3">
              <span class="fw-medium">{{ skill.name }}</span>
              <button type="button" class="btn btn-sm btn-outline-danger align-self-start align-self-sm-auto" :disabled="updating" @click="removeSkill(skill.id)">
                <i class="bi bi-trash me-1" aria-hidden="true"></i>Entfernen
              </button>
            </li>
          </ul>
          <BaseEmptyState v-else title="Noch keine Skills" message="Du hast deinem Profil noch keine Skills hinzugefügt." />
        </div>
      </div>
    </template>
  </main>
</template>
