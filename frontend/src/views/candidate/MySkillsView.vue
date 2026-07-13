<script setup>
import { computed, onMounted, ref } from "vue";
import api from "../../services/api";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";

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
    <div class="container py-4">
        <h2 class="mb-4">Meine Skills</h2>

        <div v-if="loading" class="alert alert-info">
            Skills werden geladen...
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
                    <h5>Neuen Skill hinzufügen</h5>

                    <div class="d-flex flex-wrap gap-2">
                        <select v-model="selectedSkillId" class="form-select" :disabled="updating">
                            <option value="">Skill auswählen</option>

                            <option v-for="skill in availableSkills" :key="skill.id" :value="skill.id">
                                {{ skill.name }}
                            </option>
                        </select>

                        <button type="button" class="btn btn-primary" :disabled="!selectedSkillId || updating"
                            @click="addSkill">
                            Hinzufügen
                        </button>
                    </div>
                </div>
            </div>

            <div class="card shadow-sm">
                <div class="card-body">
                    <h5>Aktuelle Skills</h5>

                    <ul v-if="hasMySkills" class="list-group">
                        <li v-for="skill in mySkills" :key="skill.id"
                            class="list-group-item d-flex justify-content-between align-items-center">
                            <span>{{ skill.name }}</span>

                            <button type="button" class="btn btn-sm btn-outline-danger" :disabled="updating"
                                @click="removeSkill(skill.id)">
                                Entfernen
                            </button>
                        </li>
                    </ul>

                    <BaseEmptyState message="Du hast noch keine Skills hinzugefügt." />
                </div>
            </div>
        </template>
    </div>
</template>