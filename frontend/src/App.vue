<script setup>
import Navbar from './components/Navbar.vue';
import Footer from './views/Footer.vue';
</script>

<template>
  <Navbar />
  <router-view></router-view>

  <Footer></Footer>
</template>