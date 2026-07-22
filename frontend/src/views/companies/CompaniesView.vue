<script setup>
import { computed, onMounted, ref } from "vue";

import { getCompanies } from "@/services/companyService";

import BaseAlert from "@/components/shared/BaseAlert.vue";
import PageHeader from "@/components/shared/PageHeader.vue";
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
    <main class="container py-4">
        <PageHeader title="Unternehmen"
            description="Entdecke Unternehmen und informiere dich über mögliche Arbeitgeber." />

        <div class="card border-0 shadow-sm mb-4">
            <div class="card-body">

                <div class="row g-2 align-items-center">
                    <div class="col-12 col-lg">
                        <input v-model="search" type="text" class="form-control" placeholder="Firma suchen..."
                            @keyup.enter="searchCompanies" />
                    </div>

                    <div class="col-12 col-sm-auto d-grid">
                        <button type="button" class="btn btn-primary" @click="searchCompanies">
                            Suchen
                        </button>
                    </div>

                    <div class="col-12 col-sm-auto d-grid">
                        <button type="button" class="btn btn-outline-secondary" @click="clearSearch">
                            Zurücksetzen
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <BaseSpinner v-if="loading" message="Firmen werden geladen..." />

        <BaseAlert v-else-if="error" type="danger" :message="error" />

        <template v-else>
            <BaseEmptyState v-if="!hasCompanies" message="Aktuell sind keine Firmen verfügbar." />

            <template v-else>
                <p class="text-body-secondary">
                    {{ totalItems }} Firmen gefunden · Seite {{ page }} von {{ totalPages }}
                </p>

                <div class="row g-3">
                    <div v-for="company in companies" :key="company.id" class="col-12 col-md-6 col-xl-4">
                        <BaseCard :title="company.name || 'Ohne Namen'">
                            <p class="text-body-secondary mb-2">
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
    </main>
</template>