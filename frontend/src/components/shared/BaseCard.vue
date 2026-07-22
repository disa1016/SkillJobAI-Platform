<script setup>
defineProps({
    title: {
        type: String,
        default: "",
    },
    fullHeight: {
        type: Boolean,
        default: true,
    },
    cardClass: {
        type: [String, Array, Object],
        default: "",
    },
    bodyClass: {
        type: [String, Array, Object],
        default: "",
    },
});
</script>

<template>
    <div class="card border-0 shadow-sm" :class="[cardClass, { 'h-100': fullHeight }]">
        <div v-if="title || $slots.header" class="card-header bg-body border-bottom">
            <slot name="header">
                <h2 class="h5 mb-0">{{ title }}</h2>
            </slot>
        </div>

        <div class="card-body" :class="bodyClass">
            <slot />
        </div>

        <div v-if="$slots.footer" class="card-footer bg-body border-top">
            <slot name="footer" />
        </div>
    </div>
</template>
