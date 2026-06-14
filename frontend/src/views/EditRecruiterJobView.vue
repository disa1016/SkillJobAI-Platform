<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import api from "../services/api";

const route = useRoute();
const router = useRouter();

const companies = ref([]);
const allSkills = ref([]);
const jobSkills = ref([]);
const selectedSkillId = ref("");

const error = ref("");
const success = ref("");
const loading = ref(true);

const form = ref({
    title: "",
    description: "",
    location: "",
    salary: "",
    companyId: "",
});

const loadJob = async () => {
    try {
        const response = await api.get(`/jobs/${route.params.id}`);

        form.value = {
            title: response.data.title,
            description: response.data.description,
            location: response.data.location,
            salary: response.data.salary,
            companyId: response.data.companyId,
        };
    } catch {
        error.value = "Job konnte nicht geladen werden.";
    }
};

const loadCompanies = async () => {
    try {
        const response = await api.get("/companies");
        companies.value = response.data;
    } catch {
        error.value = "Firmen konnten nicht geladen werden.";
    }
};

const loadSkills = async () => {
    try {
        const response = await api.get("/skills");
        allSkills.value = response.data;
    } catch {
        error.value = "Skills konnten nicht geladen werden.";
    }
};

const loadJobSkills = async () => {
    try {
        const response = await api.get(`/jobs/${route.params.id}/skills`);
        jobSkills.value = response.data;
    } catch {
        error.value = "Job Skills konnten nicht geladen werden.";
    }
};

const updateJob = async () => {
    error.value = "";
    success.value = "";

    try {
        await api.put(`/jobs/${route.params.id}`, {
            title: form.value.title,
            description: form.value.description,
            location: form.value.location,
            salary: form.value.salary,
            companyId: Number(form.value.companyId),
        });

        success.value = "Job erfolgreich aktualisiert.";
    } catch {
        error.value = "Job konnte nicht aktualisiert werden.";
    }
};

const addSkillToJob = async () => {
    if (!selectedSkillId.value) return;

    error.value = "";
    success.value = "";

    try {
        await api.post(`/jobs/${route.params.id}/skills/${selectedSkillId.value}`);

        success.value = "Skill wurde zum Job hinzugefügt.";
        selectedSkillId.value = "";

        await loadJobSkills();
    } catch (err) {
        error.value =
            err.response?.data?.message || "Skill konnte nicht hinzugefügt werden.";
    }
};

const removeSkillFromJob = async (skillId) => {
    error.value = "";
    success.value = "";

    try {
        await api.delete(`/jobs/${route.params.id}/skills/${skillId}`);

        success.value = "Skill wurde vom Job entfernt.";

        jobSkills.value = jobSkills.value.filter((skill) => skill.id !== skillId);
    } catch {
        error.value = "Skill konnte nicht entfernt werden.";
    }
};

onMounted(async () => {
    await Promise.all([
        loadJob(),
        loadCompanies(),
        loadSkills(),
        loadJobSkills(),
    ]);

    loading.value = false;
});
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Job bearbeiten</h2>

        <div v-if="loading" class="alert alert-info">
            Job wird geladen...
        </div>

        <div v-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <div v-if="success" class="alert alert-success">
            {{ success }}
        </div>

        <div v-if="!loading" class="card shadow-sm mb-4">
            <div class="card-body">
                <h4 class="mb-3">Job Details</h4>

                <div class="mb-3">
                    <label class="form-label">Titel</label>
                    <input v-model="form.title" class="form-control" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Beschreibung</label>
                    <textarea v-model="form.description" rows="5" class="form-control"></textarea>
                </div>

                <div class="mb-3">
                    <label class="form-label">Standort</label>
                    <input v-model="form.location" class="form-control" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Gehalt</label>
                    <input v-model="form.salary" class="form-control" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Firma</label>

                    <select v-model="form.companyId" class="form-select">
                        <option v-for="company in companies" :key="company.id" :value="company.id">
                            {{ company.name }}
                        </option>
                    </select>
                </div>

                <button class="btn btn-primary" @click="updateJob">
                    Änderungen speichern
                </button>
            </div>
        </div>

        <div v-if="!loading" class="card shadow-sm">
            <div class="card-body">
                <h4 class="mb-3">Benötigte Skills</h4>

                <div class="row g-2 mb-3">
                    <div class="col-md-8">
                        <select v-model="selectedSkillId" class="form-select">
                            <option value="">Skill auswählen</option>

                            <option v-for="skill in allSkills" :key="skill.id" :value="skill.id">
                                {{ skill.name }}
                            </option>
                        </select>
                    </div>

                    <div class="col-md-4">
                        <button class="btn btn-success w-100" @click="addSkillToJob" :disabled="!selectedSkillId">
                            Skill hinzufügen
                        </button>
                    </div>
                </div>

                <div v-if="jobSkills.length === 0" class="alert alert-warning">
                    Für diesen Job wurden noch keine Skills hinterlegt.
                </div>

                <ul v-else class="list-group">
                    <li v-for="skill in jobSkills" :key="skill.id"
                        class="list-group-item d-flex justify-content-between align-items-center">
                        {{ skill.name }}

                        <button class="btn btn-outline-danger btn-sm" @click="removeSkillFromJob(skill.id)">
                            Entfernen
                        </button>
                    </li>
                </ul>
            </div>
        </div>

        <button class="btn btn-secondary mt-3" @click="router.push('/recruiter/jobs')">
            Zurück
        </button>
    </div>
</template>