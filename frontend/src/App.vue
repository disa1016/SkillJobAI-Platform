<script setup>
import Navbar from "@/components/layout/Navbar.vue";
import Footer from "@/components/layout/Footer.vue";
</script>

<template>
  <div class="d-flex flex-column min-vh-100">
    <Navbar />

    <div class="flex-grow-1">
      <router-view />
    </div>

    <Footer />
  </div>
</template>
