<script setup>
defineProps({
  message: {
    type: String,
    default: "Wird geladen...",
  },
});
</script>

<template>
  <div class="text-center py-5">
    <div class="spinner-border text-primary mb-3" role="status">
      <span class="visually-hidden">Loading...</span>
    </div>

    <p class="text-muted mb-0">
      {{ message }}
    </p>
  </div>
</template>