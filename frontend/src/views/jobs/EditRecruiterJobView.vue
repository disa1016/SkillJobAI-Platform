<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import api from "../../services/api";

const route = useRoute();
const router = useRouter();

const companies = ref([]);
const allSkills = ref([]);
const jobSkills = ref([]);
const selectedSkillId = ref("");

const loading = ref(true);
const saving = ref(false);
const updatingSkills = ref(false);

const error = ref("");
const success = ref("");

const form = ref({
    title: "",
    description: "",
    location: "",
    salary: "",
    companyId: "",
});

const hasJobSkills = computed(() => jobSkills.value.length > 0);

const availableSkills = computed(() => {
    const currentSkillIds = jobSkills.value.map((skill) => skill.id);

    return allSkills.value.filter(
        (skill) => !currentSkillIds.includes(skill.id)
    );
});

const canUpdateJob = computed(() => {
    return (
        form.value.title.trim() &&
        form.value.description.trim() &&
        form.value.location.trim() &&
        form.value.companyId &&
        !saving.value
    );
});

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const getErrorMessage = (err, fallback) => {
    return err.response?.data?.message || fallback;
};

const loadJob = async () => {
    const { data } = await api.get(`/jobs/${route.params.id}`);

    form.value = {
        title: data.title || "",
        description: data.description || "",
        location: data.location || "",
        salary: data.salary || "",
        companyId: data.companyId || "",
    };
};

const loadCompanies = async () => {
    const { data } = await api.get("/companies");
    companies.value = data;
};

const loadSkills = async () => {
    const { data } = await api.get("/skills");
    allSkills.value = data;
};

const loadJobSkills = async () => {
    const { data } = await api.get(`/jobs/${route.params.id}/skills`);
    jobSkills.value = data;
};

const loadData = async () => {
    loading.value = true;
    clearMessages();

    try {
        await Promise.all([
            loadJob(),
            loadCompanies(),
            loadSkills(),
            loadJobSkills(),
        ]);
    } catch {
        error.value = "Daten konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const updateJob = async () => {
    clearMessages();

    if (!canUpdateJob.value) {
        error.value = "Bitte fülle alle Pflichtfelder aus.";
        return;
    }

    saving.value = true;

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
    } finally {
        saving.value = false;
    }
};

const addSkillToJob = async () => {
    if (!selectedSkillId.value) return;

    updatingSkills.value = true;
    clearMessages();

    try {
        await api.post(`/jobs/${route.params.id}/skills/${selectedSkillId.value}`);

        success.value = "Skill wurde zum Job hinzugefügt.";
        selectedSkillId.value = "";

        await loadJobSkills();
    } catch (err) {
        error.value = getErrorMessage(
            err,
            "Skill konnte nicht hinzugefügt werden."
        );
    } finally {
        updatingSkills.value = false;
    }
};

const removeSkillFromJob = async (skillId) => {
    updatingSkills.value = true;
    clearMessages();

    try {
        await api.delete(`/jobs/${route.params.id}/skills/${skillId}`);

        jobSkills.value = jobSkills.value.filter((skill) => skill.id !== skillId);
        success.value = "Skill wurde vom Job entfernt.";
    } catch {
        error.value = "Skill konnte nicht entfernt werden.";
    } finally {
        updatingSkills.value = false;
    }
};

const goBack = () => {
    router.push("/recruiter/jobs");
};

onMounted(loadData);
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Job bearbeiten</h2>

        <div v-if="loading" class="alert alert-info">
            Job wird geladen...
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
                    <h4 class="mb-3">Job Details</h4>

                    <form @submit.prevent="updateJob">
                        <div class="mb-3">
                            <label class="form-label">Titel</label>

                            <input v-model="form.title" class="form-control" required />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Beschreibung</label>

                            <textarea v-model="form.description" rows="5" class="form-control" required />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Standort</label>

                            <input v-model="form.location" class="form-control" required />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Gehalt</label>

                            <input v-model="form.salary" class="form-control" />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Firma</label>

                            <select v-model="form.companyId" class="form-select" required>
                                <option value="" disabled>Firma auswählen</option>

                                <option v-for="company in companies" :key="company.id" :value="company.id">
                                    {{ company.name || "Unbekannte Firma" }}
                                </option>
                            </select>
                        </div>

                        <button type="submit" class="btn btn-primary" :disabled="!canUpdateJob">
                            {{ saving ? "Wird gespeichert..." : "Änderungen speichern" }}
                        </button>
                    </form>
                </div>
            </div>

            <div class="card shadow-sm">
                <div class="card-body">
                    <h4 class="mb-3">Benötigte Skills</h4>

                    <div class="row g-2 mb-3">
                        <div class="col-md-8">
                            <select v-model="selectedSkillId" class="form-select" :disabled="updatingSkills">
                                <option value="">Skill auswählen</option>

                                <option v-for="skill in availableSkills" :key="skill.id" :value="skill.id">
                                    {{ skill.name }}
                                </option>
                            </select>
                        </div>

                        <div class="col-md-4">
                            <button type="button" class="btn btn-success w-100"
                                :disabled="!selectedSkillId || updatingSkills" @click="addSkillToJob">
                                Skill hinzufügen
                            </button>
                        </div>
                    </div>

                    <div v-if="!hasJobSkills" class="alert alert-warning">
                        Für diesen Job wurden noch keine Skills hinterlegt.
                    </div>

                    <ul v-else class="list-group">
                        <li v-for="skill in jobSkills" :key="skill.id"
                            class="list-group-item d-flex justify-content-between align-items-center">
                            {{ skill.name }}

                            <button type="button" class="btn btn-outline-danger btn-sm" :disabled="updatingSkills"
                                @click="removeSkillFromJob(skill.id)">
                                Entfernen
                            </button>
                        </li>
                    </ul>
                </div>
            </div>

            <button type="button" class="btn btn-secondary mt-3" @click="goBack">
                Zurück
            </button>
        </template>
    </div>
</template>