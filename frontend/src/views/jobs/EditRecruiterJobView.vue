<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import api from "../../services/api";
import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

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
    <main class="container py-4">
        <PageHeader title="Job bearbeiten"
            description="Aktualisiere die Stellendaten und verwalte die benötigten Skills.">
            <template #actions>
                <button type="button" class="btn btn-outline-secondary" @click="goBack"><i class="bi bi-arrow-left me-1"
                        aria-hidden="true"></i>Zurück</button>
            </template>
        </PageHeader>

        <BaseSpinner v-if="loading" message="Job wird geladen..." />

        <template v-else>
            <BaseAlert v-if="error" type="danger" :message="error" />
            <BaseAlert v-if="success" type="success" :message="success" />

            <div class="card border-0 shadow-sm mb-4">
                <div class="card-header bg-body border-bottom">
                    <h2 class="h5 mb-0">Jobdetails</h2>
                </div>
                <div class="card-body p-4">
                    <form @submit.prevent="updateJob">
                        <div class="row g-3">
                            <div class="col-12">
                                <label for="edit-job-title" class="form-label">Titel</label>
                                <input id="edit-job-title" v-model="form.title" class="form-control" required />
                            </div>
                            <div class="col-12">
                                <label for="edit-job-description" class="form-label">Beschreibung</label>
                                <textarea id="edit-job-description" v-model="form.description" rows="6"
                                    class="form-control" required></textarea>
                            </div>
                            <div class="col-12 col-md-6">
                                <label for="edit-job-location" class="form-label">Standort</label>
                                <input id="edit-job-location" v-model="form.location" class="form-control" required />
                            </div>
                            <div class="col-12 col-md-6">
                                <label for="edit-job-salary" class="form-label">Gehalt</label>
                                <input id="edit-job-salary" v-model="form.salary" class="form-control" />
                            </div>
                            <div class="col-12">
                                <label for="edit-job-company" class="form-label">Firma</label>
                                <select id="edit-job-company" v-model="form.companyId" class="form-select" required>
                                    <option value="" disabled>Firma auswählen</option>
                                    <option v-for="company in companies" :key="company.id" :value="company.id">{{
                                        company.name || "Unbekannte Firma" }}</option>
                                </select>
                            </div>
                        </div>
                        <div class="d-grid d-sm-block mt-4">
                            <button type="submit" class="btn btn-primary" :disabled="!canUpdateJob">
                                <span v-if="saving" class="spinner-border spinner-border-sm me-2"
                                    aria-hidden="true"></span>
                                {{ saving ? "Wird gespeichert..." : "Änderungen speichern" }}
                            </button>
                        </div>
                    </form>
                </div>
            </div>

            <div class="card border-0 shadow-sm">
                <div class="card-header bg-body border-bottom">
                    <h2 class="h5 mb-0">Benötigte Skills</h2>
                </div>
                <div class="card-body">
                    <div class="row g-3 align-items-end mb-4">
                        <div class="col-12 col-md-8">
                            <label for="job-skill" class="form-label">Skill</label>
                            <select id="job-skill" v-model="selectedSkillId" class="form-select"
                                :disabled="updatingSkills">
                                <option value="">Skill auswählen</option>
                                <option v-for="skill in availableSkills" :key="skill.id" :value="skill.id">{{ skill.name
                                    }}</option>
                            </select>
                        </div>
                        <div class="col-12 col-md-4 d-grid">
                            <button type="button" class="btn btn-outline-primary"
                                :disabled="!selectedSkillId || updatingSkills" @click="addSkillToJob">
                                <span v-if="updatingSkills" class="spinner-border spinner-border-sm me-2"
                                    aria-hidden="true"></span>Skill hinzufügen
                            </button>
                        </div>
                    </div>

                    <ul v-if="hasJobSkills" class="list-group list-group-flush border rounded">
                        <li v-for="skill in jobSkills" :key="skill.id"
                            class="list-group-item d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-2">
                            <span>{{ skill.name }}</span>
                            <button type="button"
                                class="btn btn-sm btn-outline-danger align-self-start align-self-sm-auto"
                                :disabled="updatingSkills" @click="removeSkillFromJob(skill.id)">Entfernen</button>
                        </li>
                    </ul>
                    <BaseEmptyState v-else title="Keine Skills hinterlegt"
                        message="Für diesen Job wurden noch keine benötigten Skills festgelegt." />
                </div>
            </div>
        </template>
    </main>
</template>
