<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import api from "../../services/api";
import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import PageHeader from "@/components/shared/PageHeader.vue";

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
    <main class="container py-4">
        <PageHeader title="Neuen Job erstellen"
            description="Erfasse die wichtigsten Informationen für eine neue Stellenausschreibung." />

        <BaseSpinner v-if="loading" message="Firmen werden geladen..." />

        <template v-else>
            <BaseAlert v-if="error" type="danger" :message="error" />
            <BaseAlert v-if="success" type="success" :message="success" />
            <BaseAlert v-if="!hasCompanies" type="warning"
                message="Es gibt noch keine Firmen. Bitte erstelle zuerst eine Firma im Company Management." />

            <div class="card border-0 shadow-sm">
                <div class="card-body p-4">
                    <form @submit.prevent="createJob">
                        <div class="row g-3">
                            <div class="col-12">
                                <label for="job-title" class="form-label">Titel</label>
                                <input id="job-title" v-model="form.title" class="form-control"
                                    placeholder="z. B. Junior .NET Developer" required />
                            </div>
                            <div class="col-12">
                                <label for="job-description" class="form-label">Beschreibung</label>
                                <textarea id="job-description" v-model="form.description" class="form-control" rows="6"
                                    placeholder="Beschreibe Aufgaben, Anforderungen und Benefits..."
                                    required></textarea>
                            </div>
                            <div class="col-12 col-md-6">
                                <label for="job-location" class="form-label">Standort</label>
                                <input id="job-location" v-model="form.location" class="form-control"
                                    placeholder="z. B. Berlin" required />
                            </div>
                            <div class="col-12 col-md-6">
                                <label for="job-salary" class="form-label">Gehalt</label>
                                <input id="job-salary" v-model="form.salary" class="form-control"
                                    placeholder="z. B. 55.000 €" />
                            </div>
                            <div class="col-12">
                                <label for="job-company" class="form-label">Firma</label>
                                <select id="job-company" v-model="form.companyId" class="form-select" required
                                    @change="onCompanyChange">
                                    <option disabled value="">Firma auswählen</option>
                                    <option v-for="company in companies" :key="company.id" :value="company.id">
                                        {{ company.name || "Unbekannte Firma" }} – {{ company.location || "Kein Standort" }}
                                    </option>
                                </select>
                            </div>
                        </div>

                        <div class="d-flex flex-column flex-sm-row gap-2 mt-4">
                            <button type="submit" class="btn btn-primary" :disabled="!canSubmit">
                                <span v-if="saving" class="spinner-border spinner-border-sm me-2"
                                    aria-hidden="true"></span>
                                {{ saving ? "Wird erstellt..." : "Job erstellen" }}
                            </button>
                            <router-link to="/recruiter/jobs" class="btn btn-outline-secondary">Abbrechen</router-link>
                        </div>
                    </form>
                </div>
            </div>
        </template>
    </main>
</template>
