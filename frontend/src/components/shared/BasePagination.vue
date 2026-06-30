<script setup>
defineProps({
    page: {
        type: Number,
        required: true,
    },
    totalPages: {
        type: Number,
        required: true,
    },
    canGoPrevious: {
        type: Boolean,
        required: true,
    },
    canGoNext: {
        type: Boolean,
        required: true,
    },
});

const emit = defineEmits([
    "previous",
    "next",
]);
</script>

<template>
    <div class="d-flex justify-content-center align-items-center gap-3 mt-4" v-if="totalPages > 1">
        <button class="btn btn-outline-primary" :disabled="!canGoPrevious" @click="emit('previous')">
             Zurück
        </button>

        <span class="fw-semibold">
            Seite {{ page }} / {{ totalPages }}
        </span>

        <button class="btn btn-outline-primary" :disabled="!canGoNext" @click="emit('next')">
            Weiter
        </button>
    </div>
</template>