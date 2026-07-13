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
        type: String,
        default: "",
    },
});
</script>

<template>
    <div class="card shadow-sm" :class="[cardClass, { 'h-100': fullHeight }]">
        <div class="card-body">

            <h5 v-if="title" class="card-title">
                {{ title }}
            </h5>

            <slot />

        </div>

        <div v-if="$slots.footer" class="card-footer bg-white border-0">
            <slot name="footer" />
        </div>
    </div>
</template>