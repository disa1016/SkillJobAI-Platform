<script setup>
defineProps({
    title: {
        type: String,
        default: "Keine Daten vorhanden",
    },
    message: {
        type: String,
        required: true,
    },
    icon: {
        type: String,
        default: "bi-inbox",
    },
});
</script>

<template>
    <div class="text-center py-5">
        <i v-if="icon" class="bi fs-1 text-body-secondary" :class="icon" aria-hidden="true"></i>

        <h2 class="h5 mt-3 mb-2">{{ title }}</h2>
        <p class="text-body-secondary mb-0">{{ message }}</p>

        <div v-if="$slots.actions" class="d-flex flex-wrap justify-content-center gap-2 mt-4">
            <slot name="actions" />
        </div>
    </div>
</template>
