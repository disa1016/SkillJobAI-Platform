<script setup>
import { computed, onMounted, ref } from "vue";
import { getCompanies } from "@/services/companyService";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";

const companies = ref([]);
const loading = ref(true);
const error = ref("");

const hasCompanies = computed(() => companies.value.length > 0);

const loadCompanies = async () => {
    loading.value = true;
    error.value = "";

    try {
        companies.value = await getCompanies();
    } catch {
        error.value = "Firmen konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

onMounted(loadCompanies);
</script>

<template>
    <div class="container mt-4">
        <h1 class="mb-4">Firmen</h1>

        <div v-if="loading" class="alert alert-info">
            Firmen werden geladen...
        </div>

        <div v-else-if="error" class="alert alert-danger">
            {{ error }}
        </div>

        <template v-else>
            <BaseEmptyState v-if="!hasCompanies" message="Aktuell sind keine Firmen verfügbar." />

            <div v-else class="row">
                <div v-for="company in companies" :key="company.id" class="col-md-4 mb-3">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <h5 class="card-title">
                                {{ company.name || "Ohne Namen" }}
                            </h5>

                            <p class="text-muted mb-2">
                                {{ company.location || "Kein Standort angegeben" }}
                            </p>

                            <p class="card-text">
                                {{ company.description || "Keine Beschreibung vorhanden." }}
                            </p>
                        </div>

                        <div class="card-footer bg-white border-0">
                            <router-link :to="`/companies/${company.id}`" class="btn btn-primary w-100">
                                Details ansehen
                            </router-link>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </div>
</template>