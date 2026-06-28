<script setup>
defineProps({
  message: {
    type: String,
    required: true,
  },
});
</script>

<template>
  <div class="alert alert-light border text-center py-4">
    <p class="mb-0 text-muted">
      {{ message }}
    </p>
  </div>
</template>