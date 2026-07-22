<template>
  <main class="container py-5">
    <div class="row justify-content-center">
      <div class="col-12 col-md-8 col-lg-6">
        <div class="text-center py-5">
          <div class="display-1 fw-bold text-primary">404</div>
          <h1 class="h3 mt-3">Seite nicht gefunden</h1>
          <p class="text-body-secondary mb-4">
            Die gesuchte Seite existiert nicht oder wurde verschoben.
          </p>
          <div class="d-flex flex-column flex-sm-row justify-content-center gap-2">
            <router-link to="/" class="btn btn-primary">
              <i class="bi bi-house-door me-2" aria-hidden="true"></i>
              Zur Startseite
            </router-link>
            <button type="button" class="btn btn-outline-secondary" @click="$router.back()">
              <i class="bi bi-arrow-left me-2" aria-hidden="true"></i>
              Zurück
            </button>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>
