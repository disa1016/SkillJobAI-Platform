<script setup>
defineProps({
    title: {
        type: String,
        required: true,
    },
    description: {
        type: String,
        default: "",
    },
    titleTag: {
        type: String,
        default: "h1",
    },
});
</script>

<template>
    <header class="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3 mb-4">
        <div>
            <component :is="titleTag" class="h3 mb-1">{{ title }}</component>
            <p v-if="description" class="text-body-secondary mb-0">
                {{ description }}
            </p>
            <slot name="description" />
        </div>

        <div v-if="$slots.actions" class="d-flex flex-column flex-sm-row flex-wrap gap-2">
            <slot name="actions" />
        </div>
    </header>
</template>
