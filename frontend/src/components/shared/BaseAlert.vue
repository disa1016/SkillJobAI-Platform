<script setup>
import { computed, useSlots } from "vue";

const props = defineProps({
  type: {
    type: String,
    default: "info",
  },
  message: {
    type: String,
    default: "",
  },
  dismissible: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["dismiss"]);
const slots = useSlots();

const isVisible = computed(() => Boolean(props.message) || Boolean(slots.default));
const alertClasses = computed(() => [
  "alert",
  `alert-${props.type}`,
  { "alert-dismissible": props.dismissible },
]);
</script>

<template>
  <div v-if="isVisible" :class="alertClasses" role="alert">
    <slot>{{ message }}</slot>

    <button v-if="dismissible" type="button" class="btn-close" aria-label="Meldung schließen"
      @click="emit('dismiss')"></button>
  </div>
</template>
