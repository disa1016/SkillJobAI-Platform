<script setup>
import { computed, onMounted, ref } from "vue";

import { getCompanies } from "@/services/companyService";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import BaseCard from "@/components/shared/BaseCard.vue";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";
import BasePagination from "@/components/shared/BasePagination.vue";

const companies = ref([]);
const loading = ref(true);
const error = ref("");

const search = ref("");
const page = ref(1);
const pageSize = ref(9);
const totalPages = ref(1);
const totalItems = ref(0);

const hasCompanies = computed(() => companies.value.length > 0);
const canGoPrevious = computed(() => page.value > 1);
const canGoNext = computed(() => page.value < totalPages.value);

const loadCompanies = async () => {
    loading.value = true;
    error.value = "";

    try {
        const data = await getCompanies({
            page: page.value,
            pageSize: pageSize.value,
            search: search.value,
        });

        companies.value = data.items;
        totalPages.value = data.totalPages;
        totalItems.value = data.totalItems;
    } catch {
        error.value = "Firmen konnten nicht geladen werden.";
    } finally {
        loading.value = false;
    }
};

const searchCompanies = async () => {
    page.value = 1;
    await loadCompanies();
};

const clearSearch = async () => {
    search.value = "";
    page.value = 1;
    await loadCompanies();
};

const goToPreviousPage = async () => {
    if (!canGoPrevious.value) return;

    page.value -= 1;
    await loadCompanies();
};

const goToNextPage = async () => {
    if (!canGoNext.value) return;

    page.value += 1;
    await loadCompanies();
};

onMounted(loadCompanies);
</script>

<template>
    <div class="container mt-4">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-4">
            <h1 class="mb-0">Firmen</h1>

            <div class="d-flex flex-wrap gap-2">
                <input v-model="search" type="text" class="form-control" style="max-width: 280px"
                    placeholder="Firma suchen..." @keyup.enter="searchCompanies" />

                <button type="button" class="btn btn-primary" @click="searchCompanies">
                    Suchen
                </button>

                <button type="button" class="btn btn-outline-secondary" @click="clearSearch">
                    Zurücksetzen
                </button>
            </div>
        </div>

        <BaseSpinner v-if="loading" message="Firmen werden geladen..." />

        <BaseAlert v-else-if="error" type="danger" :message="error" />

        <template v-else>
            <BaseEmptyState v-if="!hasCompanies" message="Aktuell sind keine Firmen verfügbar." />

            <template v-else>
                <p class="text-muted">
                    {{ totalItems }} Firmen gefunden · Seite {{ page }} von {{ totalPages }}
                </p>

                <div class="row">
                    <div v-for="company in companies" :key="company.id" class="col-md-4 mb-3">
                        <BaseCard :title="company.name || 'Ohne Namen'">
                            <p class="text-muted mb-2">
                                {{ company.location || "Kein Standort angegeben" }}
                            </p>

                            <p class="card-text">
                                {{ company.description || "Keine Beschreibung vorhanden." }}
                            </p>

                            <template #footer>
                                <router-link :to="`/companies/${company.id}`" class="btn btn-primary w-100">
                                    Details ansehen
                                </router-link>
                            </template>
                        </BaseCard>
                    </div>
                </div>

                <BasePagination :page="page" :total-pages="totalPages" :can-go-previous="canGoPrevious"
                    :can-go-next="canGoNext" @previous="goToPreviousPage" @next="goToNextPage" />
            </template>
        </template>
    </div>
</template>