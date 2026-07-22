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

const emit = defineEmits(["previous", "next"]);
</script>

<template>
  <nav v-if="totalPages > 1" class="mt-4" aria-label="Seitennavigation">
    <div class="d-flex flex-column flex-sm-row justify-content-center align-items-center gap-3">
      <button
        type="button"
        class="btn btn-outline-primary"
        :disabled="!canGoPrevious"
        @click="emit('previous')"
      >
        <i class="bi bi-chevron-left me-1" aria-hidden="true"></i>
        Zurück
      </button>

      <span class="text-body-secondary" aria-live="polite">
        Seite <span class="fw-semibold text-body">{{ page }}</span>
        von <span class="fw-semibold text-body">{{ totalPages }}</span>
      </span>

      <button
        type="button"
        class="btn btn-outline-primary"
        :disabled="!canGoNext"
        @click="emit('next')"
      >
        Weiter
        <i class="bi bi-chevron-right ms-1" aria-hidden="true"></i>
      </button>
    </div>
  </nav>
</template>
