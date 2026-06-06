<script setup>
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import api from "../services/api";

const router = useRouter();

const companies = ref([]);
const error = ref("");
const success = ref("");

const form = ref({
    title: "",
    description: "",
    location: "",
    salary: "",
    companyId: "",
});

onMounted(async () => {
    try {
        const response = await api.get("/companies");
        companies.value = response.data;
    } catch {
        error.value = "Firmen konnten nicht geladen werden.";
    }
});

const createJob = async () => {
    error.value = "";
    success.value = "";

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
    }
};
</script>

<template>
    <div class="container py-4">
        <h2 class="mb-4">Neuen Job erstellen</h2>

        <div v-if="error" class="alert alert-danger">{{ error }}</div>
        <div v-if="success" class="alert alert-success">{{ success }}</div>

        <div class="card shadow-sm">
            <div class="card-body">
                <div class="mb-3">
                    <label class="form-label">Titel</label>
                    <input v-model="form.title" class="form-control" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Beschreibung</label>
                    <textarea v-model="form.description" class="form-control" rows="5"></textarea>
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
                        <option value="">Firma auswählen</option>
                        <option v-for="company in companies" :key="company.id" :value="company.id">
                            {{ company.name }}
                        </option>
                    </select>
                </div>

                <button class="btn btn-primary" @click="createJob">
                    Job erstellen
                </button>
            </div>
        </div>
    </div>
</template>