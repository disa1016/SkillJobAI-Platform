<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import api from "../../services/api";

const router = useRouter();

const companies = ref([]);

const loading = ref(false);
const saving = ref(false);
const error = ref("");
const success = ref("");

const form = ref({
    title: "",
    description: "",
    location: "",
    salary: "",
    companyId: "",
});

const hasCompanies = computed(() => companies.value.length > 0);

const canSubmit = computed(() => {
    return (
        form.value.title.trim() &&
        form.value.description.trim() &&
        form.value.location.trim() &&
        form.value.companyId &&
        !saving.value &&
        hasCompanies.value
    );
});

const clearMessages = () => {
    error.value = "";
    success.value = "";
};

const loadCompanies = async () => {
    loading.value = true;
    clearMessages();

    try {
        const { data } = await api.get("/companies");
        companies.value = data;
    } catch {
        error.value = "Firmen konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const onCompanyChange = () => {
    const selectedCompany = companies.value.find(
        (company) => company.id === Number(form.value.companyId)
    );

    if (selectedCompany && !form.value.location) {
        form.value.location = selectedCompany.location || "";
    }
};

const validateForm = () => {
    if (!form.value.title.trim()) {
        error.value = "Bitte gib einen Jobtitel ein.";
        return false;
    }

    if (!form.value.description.trim()) {
        error.value = "Bitte gib eine Beschreibung ein.";
        return false;
    }

    if (!form.value.location.trim()) {
        error.value = "Bitte gib einen Standort ein.";
        return false;
    }

    if (!form.value.companyId) {
        error.value = "Bitte wähle eine Firma aus.";
        return false;
    }

    return true;
};

const createJob = async () => {
    clearMessages();

    if (!validateForm()) return;

    saving.value = true;

    try {
        await api.post("/jobs", {
            title: form.value.title,
            description: form.value.description,
            location: form.value.location,
            salary: form.value.salary,
            companyId: Number(form.value.companyId),
        });

        success.value = "Job wurde erstellt.";
        router.push("/recruiter/jobs");
    } catch {
        error.value = "Job konnte nicht erstellt werden.";
    } finally {
        saving.value = false;
    }
};

onMounted(loadCompanies);
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Neuen Job erstellen</h2>

        <div v-if="loading" class="alert alert-info">
            Firmen werden geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <div v-if="success" class="alert alert-success">
                {{ success }}
            </div>

            <div v-if="!hasCompanies" class="alert alert-warning">
                Es gibt noch keine Firmen. Bitte erstelle zuerst eine Firma im Company Management.
            </div>

            <div class="card shadow-sm">
                <div class="card-body">
                    <form @submit.prevent="createJob">
                        <div class="mb-3">
                            <label class="form-label">Titel</label>

                            <input v-model="form.title" class="form-control" placeholder="z.B. Junior .NET Developer"
                                required />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Beschreibung</label>

                            <textarea v-model="form.description" class="form-control" rows="5"
                                placeholder="Beschreibe Aufgaben, Anforderungen und Benefits..." required />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Standort</label>

                            <input v-model="form.location" class="form-control" placeholder="z.B. Berlin" required />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Gehalt</label>

                            <input v-model="form.salary" class="form-control" placeholder="z.B. 55.000 €" />
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Firma</label>

                            <select v-model="form.companyId" class="form-select" required @change="onCompanyChange">
                                <option disabled value="">Firma auswählen</option>

                                <option v-for="company in companies" :key="company.id" :value="company.id">
                                    {{ company.name || "Unbekannte Firma" }} -
                                    {{ company.location || "Kein Standort" }}
                                </option>
                            </select>
                        </div>

                        <div class="d-flex flex-wrap gap-2">
                            <button type="submit" class="btn btn-primary" :disabled="!canSubmit">
                                {{ saving ? "Wird erstellt..." : "Job erstellen" }}
                            </button>

                            <router-link to="/recruiter/jobs" class="btn btn-outline-secondary">
                                Abbrechen
                            </router-link>
                        </div>
                    </form>
                </div>
            </div>
        </template>
    </div>
</template>