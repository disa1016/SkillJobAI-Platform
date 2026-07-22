<template>
  <footer class="bg-dark text-white py-4">
    <div
      class="container d-flex flex-column flex-md-row justify-content-between align-items-center gap-3"
    >
      <p class="mb-0 text-center text-md-start">
        © 2026 SkillJob AI
      </p>

      <nav
        class="d-flex flex-column flex-sm-row align-items-center gap-2 gap-sm-3"
        aria-label="Footer-Navigation"
      >
        <router-link class="link-light text-decoration-none" to="/impressum">
          Impressum
        </router-link>

        <router-link class="link-light text-decoration-none" to="/privacy">
          Datenschutz
        </router-link>

        <router-link class="link-light text-decoration-none" to="/contact">
          Kontakt
        </router-link>
      </nav>
    </div>
  </footer>
</template>
