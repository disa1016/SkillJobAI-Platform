<script setup>
import { computed } from "vue";

const props = defineProps({
    message: {
        type: String,
        default: "",
    },
    text: {
        type: String,
        default: "",
    },
});

const displayMessage = computed(
    () => props.message || props.text || "Wird geladen...",
);
</script>

<template>
    <div class="d-flex flex-column align-items-center justify-content-center py-5" aria-live="polite">
        <div class="spinner-border" role="status">
            <span class="visually-hidden">{{ displayMessage }}</span>
        </div>

        <p class="text-body-secondary mb-0 mt-3">{{ displayMessage }}</p>
    </div>
</template>
