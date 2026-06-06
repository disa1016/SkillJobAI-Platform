<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import api from "../services/api";

const route = useRoute();
const router = useRouter();

const companies = ref([]);
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

    setTimeout(() => {
      router.push("/recruiter/jobs");
    }, 1000);
  } catch {
    error.value = "Job konnte nicht aktualisiert werden.";
  }
};

onMounted(async () => {
  await Promise.all([
    loadJob(),
    loadCompanies()
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

    <div v-if="!loading" class="card shadow-sm">
      <div class="card-body">

        <div class="mb-3">
          <label class="form-label">Titel</label>
          <input
            v-model="form.title"
            class="form-control"
          />
        </div>

        <div class="mb-3">
          <label class="form-label">Beschreibung</label>
          <textarea
            v-model="form.description"
            rows="5"
            class="form-control"
          ></textarea>
        </div>

        <div class="mb-3">
          <label class="form-label">Standort</label>
          <input
            v-model="form.location"
            class="form-control"
          />
        </div>

        <div class="mb-3">
          <label class="form-label">Gehalt</label>
          <input
            v-model="form.salary"
            class="form-control"
          />
        </div>

        <div class="mb-3">
          <label class="form-label">Firma</label>

          <select
            v-model="form.companyId"
            class="form-select"
          >
            <option
              v-for="company in companies"
              :key="company.id"
              :value="company.id"
            >
              {{ company.name }}
            </option>
          </select>
        </div>

        <button
          class="btn btn-primary"
          @click="updateJob"
        >
          Änderungen speichern
        </button>

      </div>
    </div>
  </div>
</template>