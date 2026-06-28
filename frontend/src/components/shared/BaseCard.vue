<script setup>
defineProps({
    title: {
        type: String,
        default: "",
    },
});
</script>

<template>
    <div class="card shadow-sm h-100">
        <div class="card-body">

            <h5 v-if="title" class="card-title">
                {{ title }}
            </h5>

            <slot />

        </div>
    </div>
</template>