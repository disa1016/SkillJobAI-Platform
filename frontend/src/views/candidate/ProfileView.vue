<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from "vue";

import {
  deleteCv,
  deleteProfileImage,
  downloadCourseCertificate,
  getMyApplications,
  getMyEnrollments,
  getMyProgress,
  getProfile,
  updateProfile,
  uploadCv,
  uploadProfileImage,
} from "@/services/candidateService";

import api from "@/services/api";
import { formatDate } from "@/utils/date";
import BaseEmptyState from "@/components/shared/BaseEmptyState.vue";
import BaseSpinner from "@/components/shared/BaseSpinner.vue";

const MAX_CV_SIZE = 5 * 1024 * 1024;
const MAX_PROFILE_IMAGE_SIZE = 10 * 1024 * 1024;

const user = ref(JSON.parse(localStorage.getItem("user")) || null);

const applications = ref([]);
const enrollments = ref([]);
const progress = ref([]);

const selectedCv = ref(null);
const cvInput = ref(null);

const selectedProfileImage = ref(null);
const profileImageInput = ref(null);
const profileImagePreviewUrl = ref("");

const loading = ref(true);
const uploadingCv = ref(false);
const deletingCv = ref(false);
const uploadingProfileImage = ref(false);
const deletingProfileImage = ref(false);
const downloadingCertificateId = ref(null);
const editingProfile = ref(false);
const savingProfile = ref(false);

const error = ref("");
const cvMessage = ref("");
const cvError = ref("");
const certificateError = ref("");
const profileMessage = ref("");
const profileError = ref("");
const profileImageMessage = ref("");
const profileImageError = ref("");
const profileValidationErrors = ref({});

const profileForm = ref({
  fullName: "",
  phoneNumber: "",
  location: "",
  headline: "",
  about: "",
  linkedInUrl: "",
  githubUrl: "",
  website: "",
});

const backendUrl = computed(() => {
  const baseUrl = api.defaults.baseURL || "";
  return baseUrl.replace(/\/api\/?$/, "");
});

const cvFullUrl = computed(() => {
  const cvUrl = user.value?.cvUrl;

  if (!cvUrl) return "";
  if (cvUrl.startsWith("http")) return cvUrl;

  return `${backendUrl.value}${cvUrl}`;
});

const profileImageFullUrl = computed(() => {
  const profileImageUrl = user.value?.profileImageUrl;

  if (!profileImageUrl) return "";
  if (profileImageUrl.startsWith("http")) return profileImageUrl;

  return `${backendUrl.value}${profileImageUrl}`;
});

const displayedProfileImageUrl = computed(() => {
  return profileImagePreviewUrl.value || profileImageFullUrl.value;
});

const normalizedRole = computed(() => {
  return user.value?.role?.trim().toLowerCase() || "";
});

const isCandidate = computed(() => {
  return (
    normalizedRole.value === "candidate" ||
    normalizedRole.value === "student"
  );
});

const roleLabel = computed(() => {
  const roleLabels = {
    candidate: "Candidate",
    student: "Candidate",
    recruiter: "Recruiter",
    admin: "Administrator",
  };

  return roleLabels[normalizedRole.value] || user.value?.role || "Keine Rolle";
});

const userInitials = computed(() => {
  const fullName = user.value?.fullName?.trim();

  if (!fullName) return "U";

  return fullName
    .split(/\s+/)
    .slice(0, 2)
    .map((namePart) => namePart.charAt(0).toUpperCase())
    .join("");
});

const profileCompletion = computed(() => {
  const fields = [
    Boolean(user.value?.fullName),
    Boolean(user.value?.email),
    Boolean(user.value?.role),
    Boolean(user.value?.phoneNumber),
    Boolean(user.value?.location),
    Boolean(user.value?.headline),
    Boolean(user.value?.about),
    Boolean(user.value?.linkedInUrl),
    Boolean(user.value?.githubUrl),
    Boolean(user.value?.website),
    Boolean(user.value?.profileImageUrl),
  ];

  if (isCandidate.value) {
    fields.push(Boolean(user.value?.cvUrl));
  }

  const completedFields = fields.filter(Boolean).length;

  return Math.round((completedFields / fields.length) * 100);
});

const hasApplications = computed(() => applications.value.length > 0);
const hasEnrollments = computed(() => enrollments.value.length > 0);
const hasProgress = computed(() => progress.value.length > 0);

const completedCoursesCount = computed(() => {
  return enrollments.value.filter((enrollment) => {
    const progressValue =
      enrollment.progress ??
      enrollment.progressPercentage ??
      enrollment.completionPercentage;

    return Number(progressValue) >= 100;
  }).length;
});

const selectedCvName = computed(() => {
  return selectedCv.value?.name || "";
});

const formatDateTime = (date) => {
  if (!date) return "Kein Datum";

  return new Date(date).toLocaleString("de-DE");
};

const clearCvMessages = () => {
  cvMessage.value = "";
  cvError.value = "";
};

const resetCvInput = () => {
  selectedCv.value = null;

  if (cvInput.value) {
    cvInput.value.value = "";
  }
};

const clearProfileImageMessages = () => {
  profileImageMessage.value = "";
  profileImageError.value = "";
};

const revokeProfileImagePreview = () => {
  if (profileImagePreviewUrl.value) {
    window.URL.revokeObjectURL(profileImagePreviewUrl.value);
    profileImagePreviewUrl.value = "";
  }
};

const resetProfileImageInput = () => {
  selectedProfileImage.value = null;
  revokeProfileImagePreview();

  if (profileImageInput.value) {
    profileImageInput.value.value = "";
  }
};

const handleProfileImageChange = async (event) => {
  clearProfileImageMessages();
  revokeProfileImagePreview();

  const file = event.target.files?.[0];

  if (!file) {
    selectedProfileImage.value = null;
    return;
  }

  const allowedTypes = ["image/jpeg", "image/png"];
  const fileName = file.name.toLowerCase();
  const hasAllowedExtension =
    fileName.endsWith(".jpg") ||
    fileName.endsWith(".jpeg") ||
    fileName.endsWith(".png");

  if (!allowedTypes.includes(file.type) && !hasAllowedExtension) {
    resetProfileImageInput();
    profileImageError.value =
      "Bitte nur JPG-, JPEG- oder PNG-Dateien auswählen.";
    return;
  }

  if (file.size > MAX_PROFILE_IMAGE_SIZE) {
    resetProfileImageInput();
    profileImageError.value =
      "Das Profilbild darf maximal 2 MB groß sein.";
    return;
  }

  selectedProfileImage.value = file;
  profileImagePreviewUrl.value = window.URL.createObjectURL(file);

  await handleUploadProfileImage();
};

const handleUploadProfileImage = async () => {
  clearProfileImageMessages();

  if (!selectedProfileImage.value) {
    profileImageError.value = "Bitte zuerst ein Profilbild auswählen.";
    return;
  }

  uploadingProfileImage.value = true;

  try {
    await uploadProfileImage(selectedProfileImage.value);
    await loadProfile();

    resetProfileImageInput();
    profileImageMessage.value =
      "Profilbild wurde erfolgreich hochgeladen.";
  } catch (err) {
    profileImageError.value =
      err.response?.data?.message ||
      "Profilbild konnte nicht hochgeladen werden.";
  } finally {
    uploadingProfileImage.value = false;
  }
};

const handleDeleteProfileImage = async () => {
  clearProfileImageMessages();

  const confirmed = window.confirm(
    "Möchtest du dein Profilbild wirklich löschen?"
  );

  if (!confirmed) return;

  deletingProfileImage.value = true;

  try {
    await deleteProfileImage();
    await loadProfile();

    resetProfileImageInput();
    profileImageMessage.value =
      "Profilbild wurde erfolgreich gelöscht.";
  } catch (err) {
    profileImageError.value =
      err.response?.data?.message ||
      "Profilbild konnte nicht gelöscht werden.";
  } finally {
    deletingProfileImage.value = false;
  }
};

const fillProfileForm = (profile) => {
  profileForm.value = {
    fullName: profile?.fullName ?? "",
    phoneNumber: profile?.phoneNumber ?? "",
    location: profile?.location ?? "",
    headline: profile?.headline ?? "",
    about: profile?.about ?? "",
    linkedInUrl: profile?.linkedInUrl ?? "",
    githubUrl: profile?.githubUrl ?? "",
    website: profile?.website ?? "",
  };
};

const loadProfile = async () => {
  const data = await getProfile();

  user.value = data;
  fillProfileForm(data);
  localStorage.setItem("user", JSON.stringify(data));
};

const startProfileEditing = () => {
  fillProfileForm(user.value);
  profileMessage.value = "";
  profileError.value = "";
  profileValidationErrors.value = {};
  editingProfile.value = true;
};

const cancelProfileEditing = () => {
  fillProfileForm(user.value);
  profileError.value = "";
  profileValidationErrors.value = {};
  editingProfile.value = false;
};

const getProfileFieldError = (fieldName) => {
  const pascalCaseName =
    fieldName.charAt(0).toUpperCase() + fieldName.slice(1);

  const errors =
    profileValidationErrors.value[fieldName] ??
    profileValidationErrors.value[pascalCaseName];

  return Array.isArray(errors) ? errors[0] : "";
};

const handleUpdateProfile = async () => {
  profileMessage.value = "";
  profileError.value = "";
  profileValidationErrors.value = {};

  if (!profileForm.value.fullName.trim()) {
    profileError.value = "Der vollständige Name ist erforderlich.";
    return;
  }

  savingProfile.value = true;

  try {
    const payload = {
      fullName: profileForm.value.fullName.trim(),
      phoneNumber: profileForm.value.phoneNumber.trim(),
      location: profileForm.value.location.trim(),
      headline: profileForm.value.headline.trim(),
      about: profileForm.value.about.trim(),
      linkedInUrl: profileForm.value.linkedInUrl.trim(),
      githubUrl: profileForm.value.githubUrl.trim(),
      website: profileForm.value.website.trim(),
    };

    const response = await updateProfile(payload);
    const updatedUser = response?.user ?? response;

    user.value = updatedUser;
    fillProfileForm(updatedUser);
    localStorage.setItem("user", JSON.stringify(updatedUser));

    editingProfile.value = false;
    profileMessage.value =
      response?.message || "Profil wurde erfolgreich aktualisiert.";
  } catch (err) {
    const response = err.response;

    if (response?.status === 400 && response.data?.errors) {
      profileValidationErrors.value = response.data.errors;
      profileError.value = "Bitte überprüfe deine Eingaben.";
    } else if (response?.status === 401) {
      profileError.value =
        "Deine Anmeldung ist abgelaufen. Bitte melde dich erneut an.";
    } else {
      profileError.value =
        response?.data?.message ||
        "Profil konnte nicht aktualisiert werden.";
    }
  } finally {
    savingProfile.value = false;
  }
};

const loadProfileData = async () => {
  loading.value = true;
  error.value = "";

  try {
    await loadProfile();

    if (isCandidate.value) {
      const [
        applicationsData,
        enrollmentsData,
        progressData,
      ] = await Promise.all([
        getMyApplications(),
        getMyEnrollments(),
        getMyProgress(),
      ]);

      applications.value = applicationsData ?? [];
      enrollments.value = enrollmentsData ?? [];
      progress.value = progressData ?? [];
    } else {
      applications.value = [];
      enrollments.value = [];
      progress.value = [];
    }
  } catch (err) {
    console.error("Profil konnte nicht geladen werden:", {
      status: err.response?.status,
      data: err.response?.data,
      message: err.message,
    });

    error.value =
      err.response?.data?.message ||
      "Profil-Daten konnten nicht geladen werden.";
  } finally {
    loading.value = false;
  }
};

const handleCvChange = (event) => {
  clearCvMessages();

  const file = event.target.files?.[0];

  if (!file) {
    selectedCv.value = null;
    return;
  }

  const isPdf =
    file.type === "application/pdf" ||
    file.name.toLowerCase().endsWith(".pdf");

  if (!isPdf) {
    resetCvInput();
    cvError.value = "Bitte nur PDF-Dateien hochladen.";
    return;
  }

  if (file.size > MAX_CV_SIZE) {
    resetCvInput();
    cvError.value = "Die PDF-Datei darf maximal 5 MB groß sein.";
    return;
  }

  selectedCv.value = file;
};

const handleUploadCv = async () => {
  clearCvMessages();

  if (!selectedCv.value) {
    cvError.value = "Bitte zuerst eine PDF-Datei auswählen.";
    return;
  }

  uploadingCv.value = true;

  try {
    await uploadCv(selectedCv.value);
    await loadProfile();

    resetCvInput();
    cvMessage.value = "Lebenslauf wurde erfolgreich hochgeladen.";
  } catch (err) {
    cvError.value =
      err.response?.data?.message ||
      "Lebenslauf konnte nicht hochgeladen werden.";
  } finally {
    uploadingCv.value = false;
  }
};

const handleDeleteCv = async () => {
  clearCvMessages();

  const confirmed = window.confirm(
    "Möchtest du deinen Lebenslauf wirklich löschen?"
  );

  if (!confirmed) return;

  deletingCv.value = true;

  try {
    await deleteCv();
    await loadProfile();

    resetCvInput();
    cvMessage.value = "Lebenslauf wurde erfolgreich gelöscht.";
  } catch (err) {
    cvError.value =
      err.response?.data?.message ||
      "Lebenslauf konnte nicht gelöscht werden.";
  } finally {
    deletingCv.value = false;
  }
};

const downloadCertificate = async (courseId, courseTitle) => {
  certificateError.value = "";
  downloadingCertificateId.value = courseId;

  try {
    const blobData = await downloadCourseCertificate(courseId);

    const blob = new Blob([blobData], {
      type: "application/pdf",
    });

    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = `certificate-${courseTitle || courseId}.pdf`;

    document.body.appendChild(link);
    link.click();
    link.remove();

    window.URL.revokeObjectURL(url);
  } catch (err) {
    if (err.response?.data instanceof Blob) {
      try {
        const text = await err.response.data.text();
        const data = JSON.parse(text);

        certificateError.value =
          data.message ||
          "Zertifikat konnte nicht heruntergeladen werden.";
      } catch {
        certificateError.value =
          "Zertifikat konnte nicht heruntergeladen werden.";
      }

      return;
    }

    certificateError.value =
      err.response?.data?.message ||
      "Zertifikat konnte nicht heruntergeladen werden.";
  } finally {
    downloadingCertificateId.value = null;
  }
};

onMounted(loadProfileData);
onBeforeUnmount(revokeProfileImagePreview);
</script>

<template>
  <main class="container py-4">
    <BaseSpinner v-if="loading" message="Profil wird geladen..." class="py-5" />

    <div v-else-if="error" class="alert alert-danger" role="alert">
      <div class="d-flex flex-column flex-md-row align-items-md-center gap-3">
        <div class="flex-grow-1">
          <strong>Profil konnte nicht geladen werden.</strong>
          <div class="mt-1">{{ error }}</div>
        </div>

        <button type="button" class="btn btn-outline-danger" @click="loadProfileData">
          Erneut versuchen
        </button>
      </div>
    </div>

    <template v-else>
      <div v-if="certificateError" class="alert alert-warning" role="alert">
        {{ certificateError }}
      </div>

      <section class="card border-0 shadow-sm mb-4">
        <div class="card-body p-4">
          <div class="row align-items-center g-4">
            <div class="col-12 col-md-auto text-center">
              <label for="profileImageFile" class="position-relative d-inline-block" role="button" tabindex="0"
                title="Profilbild ändern" aria-label="Profilbild ändern"
                @keydown.enter.prevent="profileImageInput?.click()" @keydown.space.prevent="profileImageInput?.click()">
                <img v-if="displayedProfileImageUrl" :src="displayedProfileImageUrl"
                  :alt="`Profilbild von ${user?.fullName || 'Benutzer'}`" width="128" height="128"
                  class="rounded-circle object-fit-cover border shadow-sm" />

                <span v-else
                  class="d-inline-flex align-items-center justify-content-center rounded-circle bg-body-tertiary border shadow-sm fs-2 fw-semibold p-4">
                  {{ userInitials }}
                </span>

                <span
                  class="position-absolute bottom-0 end-0 d-flex align-items-center justify-content-center rounded-circle bg-body border shadow-sm p-2"
                  aria-hidden="true">
                  <i class="bi bi-camera"></i>
                </span>

                <span v-if="uploadingProfileImage"
                  class="position-absolute top-50 start-50 translate-middle spinner-border" role="status">
                  <span class="visually-hidden">Profilbild wird hochgeladen...</span>
                </span>
              </label>

              <input id="profileImageFile" ref="profileImageInput" type="file"
                accept="image/jpeg,image/png,.jpg,.jpeg,.png" class="visually-hidden"
                :disabled="uploadingProfileImage || deletingProfileImage" @change="handleProfileImageChange" />

              <div v-if="user?.profileImageUrl" class="mt-2">
                <button type="button" class="btn btn-outline-danger btn-sm"
                  :disabled="uploadingProfileImage || deletingProfileImage" @click="handleDeleteProfileImage">
                  <span v-if="deletingProfileImage" class="spinner-border spinner-border-sm me-2"></span>
                  {{ deletingProfileImage ? "Wird entfernt..." : "Foto entfernen" }}
                </button>
              </div>
            </div>

            <div class="col-12 col-md">
              <div class="d-flex flex-wrap align-items-center gap-2 mb-2">
                <h1 class="h3 mb-0">
                  {{ user?.fullName || "Unbekannter Benutzer" }}
                </h1>
                <span class="badge text-bg-primary">{{ roleLabel }}</span>
              </div>

              <p class="text-body-secondary mb-1">
                {{ user?.email || "Keine E-Mail hinterlegt" }}
              </p>
              <p v-if="user?.headline" class="mb-1">{{ user.headline }}</p>
              <p v-if="user?.createdAt" class="small text-body-secondary mb-0">
                Mitglied seit {{ formatDate(user.createdAt) }}
              </p>
            </div>

            <div class="col-12 col-lg-4">
              <div class="d-flex justify-content-between mb-2">
                <span class="fw-semibold">Profilstatus</span>
                <span>{{ profileCompletion }} %</span>
              </div>
              <div class="progress" role="progressbar" :aria-valuenow="profileCompletion" aria-valuemin="0"
                aria-valuemax="100">
                <div class="progress-bar" :style="{ width: `${profileCompletion}%` }"></div>
              </div>
              <p class="small text-body-secondary mb-0 mt-2">
                {{
                  profileCompletion === 100
                    ? "Dein Profil ist vollständig."
                    : "Vervollständige dein Profil für bessere Ergebnisse."
                }}
              </p>
            </div>
          </div>
        </div>
      </section>

      <div class="row g-4">
        <div :class="isCandidate ? 'col-12 col-lg-5' : 'col-12'">
          <section class="card border-0 shadow-sm h-100">
            <div class="card-header bg-body border-bottom py-3">
              <div class="d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-2">
                <div>
                  <h2 class="h5 mb-1">Persönliche Informationen</h2>
                  <p class="small text-body-secondary mb-0">Kontaktdaten und öffentliche Profilangaben</p>
                </div>
                <button v-if="!editingProfile" type="button" class="btn btn-outline-primary btn-sm"
                  @click="startProfileEditing">
                  <i class="bi bi-pencil me-1"></i>
                  Profil bearbeiten
                </button>
              </div>
            </div>

            <div class="card-body">
              <div v-if="profileImageMessage" class="alert alert-success" role="alert">
                {{ profileImageMessage }}
              </div>
              <div v-if="profileImageError" class="alert alert-danger" role="alert">
                {{ profileImageError }}
              </div>
              <div v-if="profileMessage" class="alert alert-success" role="alert">
                {{ profileMessage }}
              </div>
              <div v-if="profileError" class="alert alert-danger" role="alert">
                {{ profileError }}
              </div>

              <form v-if="editingProfile" @submit.prevent="handleUpdateProfile">
                <div class="row g-3">
                  <div class="col-12">
                    <label for="profileFullName" class="form-label">Vollständiger Name</label>
                    <input id="profileFullName" v-model="profileForm.fullName" type="text" class="form-control"
                      :class="{ 'is-invalid': getProfileFieldError('fullName') }" autocomplete="name" required />
                    <div class="invalid-feedback">{{ getProfileFieldError("fullName") }}</div>
                  </div>

                  <div class="col-12 col-md-6">
                    <label for="profilePhoneNumber" class="form-label">Telefonnummer</label>
                    <input id="profilePhoneNumber" v-model="profileForm.phoneNumber" type="tel" class="form-control"
                      :class="{ 'is-invalid': getProfileFieldError('phoneNumber') }" placeholder="+491234567890"
                      autocomplete="tel" />
                    <div class="invalid-feedback">{{ getProfileFieldError("phoneNumber") }}</div>
                  </div>

                  <div class="col-12 col-md-6">
                    <label for="profileLocation" class="form-label">Standort</label>
                    <input id="profileLocation" v-model="profileForm.location" type="text" class="form-control"
                      :class="{ 'is-invalid': getProfileFieldError('location') }" placeholder="Berlin" />
                    <div class="invalid-feedback">{{ getProfileFieldError("location") }}</div>
                  </div>

                  <div class="col-12">
                    <label for="profileHeadline" class="form-label">Berufsbezeichnung</label>
                    <input id="profileHeadline" v-model="profileForm.headline" type="text" class="form-control"
                      :class="{ 'is-invalid': getProfileFieldError('headline') }" placeholder="Full-Stack Developer" />
                    <div class="invalid-feedback">{{ getProfileFieldError("headline") }}</div>
                  </div>

                  <div class="col-12">
                    <label for="profileAbout" class="form-label">Über mich</label>
                    <textarea id="profileAbout" v-model="profileForm.about" class="form-control"
                      :class="{ 'is-invalid': getProfileFieldError('about') }" rows="5"
                      placeholder="Erzähle etwas über dich..."></textarea>
                    <div class="invalid-feedback">{{ getProfileFieldError("about") }}</div>
                  </div>

                  <div class="col-12">
                    <label for="profileLinkedIn" class="form-label">LinkedIn</label>
                    <input id="profileLinkedIn" v-model="profileForm.linkedInUrl" type="url" class="form-control"
                      :class="{ 'is-invalid': getProfileFieldError('linkedInUrl') }"
                      placeholder="https://www.linkedin.com/in/..." />
                    <div class="invalid-feedback">{{ getProfileFieldError("linkedInUrl") }}</div>
                  </div>

                  <div class="col-12 col-md-6">
                    <label for="profileGithub" class="form-label">GitHub</label>
                    <input id="profileGithub" v-model="profileForm.githubUrl" type="url" class="form-control"
                      :class="{ 'is-invalid': getProfileFieldError('githubUrl') }"
                      placeholder="https://github.com/..." />
                    <div class="invalid-feedback">{{ getProfileFieldError("githubUrl") }}</div>
                  </div>

                  <div class="col-12 col-md-6">
                    <label for="profileWebsite" class="form-label">Website</label>
                    <input id="profileWebsite" v-model="profileForm.website" type="url" class="form-control"
                      :class="{ 'is-invalid': getProfileFieldError('website') }" placeholder="https://example.com" />
                    <div class="invalid-feedback">{{ getProfileFieldError("website") }}</div>
                  </div>
                </div>

                <div class="d-flex flex-column flex-sm-row justify-content-end gap-2 mt-4">
                  <button type="button" class="btn btn-outline-secondary" :disabled="savingProfile"
                    @click="cancelProfileEditing">
                    Abbrechen
                  </button>
                  <button type="submit" class="btn btn-primary" :disabled="savingProfile">
                    <span v-if="savingProfile" class="spinner-border spinner-border-sm me-2"></span>
                    {{ savingProfile ? "Wird gespeichert..." : "Änderungen speichern" }}
                  </button>
                </div>
              </form>

              <div v-else class="list-group list-group-flush">
                <div class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-person text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">Vollständiger Name</small><span>{{ user?.fullName ||
                      "Nicht angegeben" }}</span></div>
                </div>
                <div class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-envelope text-body-secondary"></i>
                  <div class="overflow-hidden"><small class="text-body-secondary d-block">E-Mail-Adresse</small><span
                      class="text-break">{{ user?.email || "Nicht angegeben" }}</span></div>
                </div>
                <div class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-telephone text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">Telefonnummer</small><span>{{ user?.phoneNumber ||
                      "Nicht angegeben" }}</span></div>
                </div>
                <div class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-geo-alt text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">Standort</small><span>{{ user?.location || "Nicht angegeben" }}</span></div>
                </div>
                <div class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-briefcase text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">Berufsbezeichnung</small><span>{{ user?.headline ||
                      "Nicht angegeben" }}</span></div>
                </div>
                <div v-if="user?.about" class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-info-circle text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">Über mich</small><span class="text-break">{{
                      user.about }}</span></div>
                </div>
                <div class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-shield-check text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">Benutzerrolle</small><span>{{ roleLabel }}</span>
                  </div>
                </div>
                <div v-if="user?.linkedInUrl" class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-linkedin text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">LinkedIn</small><a :href="user.linkedInUrl"
                      target="_blank" rel="noopener noreferrer">Profil öffnen</a></div>
                </div>
                <div v-if="user?.githubUrl" class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-github text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">GitHub</small><a :href="user.githubUrl"
                      target="_blank" rel="noopener noreferrer">Profil öffnen</a></div>
                </div>
                <div v-if="user?.website" class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-globe text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">Website</small><a :href="user.website" target="_blank"
                      rel="noopener noreferrer">Website öffnen</a></div>
                </div>
                <div v-if="user?.createdAt" class="list-group-item px-0 d-flex gap-3">
                  <i class="bi bi-calendar3 text-body-secondary"></i>
                  <div><small class="text-body-secondary d-block">Registriert am</small><span>{{
                    formatDate(user.createdAt) }}</span></div>
                </div>
              </div>
            </div>
          </section>
        </div>

        <div v-if="isCandidate" class="col-12 col-lg-7">
          <section class="card border-0 shadow-sm h-100">
            <div class="card-header bg-body border-bottom py-3">
              <div class="d-flex justify-content-between align-items-center gap-3">
                <div>
                  <h2 class="h5 mb-1">Lebenslauf</h2>
                  <p class="small text-body-secondary mb-0">PDF-Dokument für Bewerbungen</p>
                </div>
                <span :class="user?.cvUrl ? 'badge text-bg-success' : 'badge text-bg-secondary'">
                  {{ user?.cvUrl ? "Hochgeladen" : "Nicht vorhanden" }}
                </span>
              </div>
            </div>

            <div class="card-body">
              <div v-if="cvMessage" class="alert alert-success" role="alert">{{ cvMessage }}</div>
              <div v-if="cvError" class="alert alert-danger" role="alert">{{ cvError }}</div>

              <div v-if="user?.cvUrl" class="border rounded p-3 mb-4">
                <div class="d-flex flex-column flex-sm-row align-items-sm-center gap-3">
                  <i class="bi bi-file-earmark-pdf fs-1 text-danger"></i>
                  <div class="flex-grow-1">
                    <strong class="d-block">Dein aktueller Lebenslauf</strong>
                    <span class="small text-body-secondary">Bereit für Bewerbungen</span>
                  </div>
                  <div class="d-flex flex-column flex-sm-row gap-2">
                    <a :href="cvFullUrl" target="_blank" rel="noopener noreferrer" class="btn btn-outline-primary">CV
                      anzeigen</a>
                    <button type="button" class="btn btn-outline-danger" :disabled="deletingCv" @click="handleDeleteCv">
                      <span v-if="deletingCv" class="spinner-border spinner-border-sm me-2"></span>
                      {{ deletingCv ? "Wird gelöscht..." : "CV löschen" }}
                    </button>
                  </div>
                </div>
              </div>

              <BaseEmptyState v-else title="Noch kein Lebenslauf vorhanden"
                message="Lade einen PDF-Lebenslauf hoch, um dich direkt bewerben zu können." icon="bi-file-earmark-pdf"
                class="mb-4" />

              <div class="border rounded p-3">
                <label for="cvFile" class="form-label">Neuen Lebenslauf auswählen</label>
                <input id="cvFile" ref="cvInput" type="file" accept="application/pdf,.pdf" class="form-control"
                  @change="handleCvChange" />
                <div v-if="selectedCvName" class="small mt-2">
                  Ausgewählte Datei: <strong>{{ selectedCvName }}</strong>
                </div>

                <div class="d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 mt-3">
                  <small class="text-body-secondary">PDF-Datei, maximal 5 MB.</small>
                  <button type="button" class="btn btn-primary" :disabled="!selectedCv || uploadingCv"
                    @click="handleUploadCv">
                    <span v-if="uploadingCv" class="spinner-border spinner-border-sm me-2"></span>
                    {{ uploadingCv ? "Wird hochgeladen..." : "Lebenslauf hochladen" }}
                  </button>
                </div>
              </div>
            </div>
          </section>
        </div>
      </div>

      <template v-if="isCandidate">
        <section class="mt-4">
          <div class="mb-3">
            <h2 class="h4 mb-1">Deine Aktivitäten</h2>
            <p class="text-body-secondary mb-0">Bewerbungen, Kurse und abgeschlossene Lektionen</p>
          </div>

          <div class="row g-3">
            <div class="col-12 col-md-4">
              <div class="card border-0 shadow-sm h-100">
                <div class="card-body">
                  <div class="d-flex align-items-center gap-3">
                    <i class="bi bi-send fs-2 text-body-secondary"></i>
                    <div>
                      <div class="display-6 fw-semibold">{{ applications.length }}</div>
                      <div class="text-body-secondary">Bewerbungen</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 col-md-4">
              <div class="card border-0 shadow-sm h-100">
                <div class="card-body">
                  <div class="d-flex align-items-center gap-3">
                    <i class="bi bi-book fs-2 text-body-secondary"></i>
                    <div>
                      <div class="display-6 fw-semibold">{{ enrollments.length }}</div>
                      <div class="text-body-secondary">Kurse</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-12 col-md-4">
              <div class="card border-0 shadow-sm h-100">
                <div class="card-body">
                  <div class="d-flex align-items-center gap-3">
                    <i class="bi bi-check-circle fs-2 text-body-secondary"></i>
                    <div>
                      <div class="display-6 fw-semibold">{{ progress.length }}</div>
                      <div class="text-body-secondary">Lektionen</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <div class="row g-4 mt-1">
          <div class="col-12 col-lg-6">
            <section class="card border-0 shadow-sm h-100">
              <div class="card-header bg-body border-bottom py-3 d-flex justify-content-between align-items-center">
                <h2 class="h5 mb-0">Meine Bewerbungen</h2>
                <span class="badge text-bg-secondary">{{ applications.length }}</span>
              </div>
              <div class="card-body p-0">
                <div v-if="hasApplications" class="list-group list-group-flush">
                  <div v-for="application in applications" :key="application.id" class="list-group-item py-3">
                    <div class="d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-2">
                      <div>
                        <strong class="d-block">{{ application.job?.title || "Job gelöscht" }}</strong>
                        <span class="small text-body-secondary">{{ application.job?.company || "Keine Firma" }}</span>
                      </div>
                      <span class="badge text-bg-secondary">{{ application.status || "Unbekannt" }}</span>
                    </div>
                  </div>
                </div>
                <BaseEmptyState v-else title="Noch keine Bewerbungen"
                  message="Deine eingereichten Bewerbungen werden hier angezeigt." icon="bi-send" />
              </div>
            </section>
          </div>

          <div class="col-12 col-lg-6">
            <section class="card border-0 shadow-sm h-100">
              <div class="card-header bg-body border-bottom py-3 d-flex justify-content-between align-items-center">
                <h2 class="h5 mb-0">Meine Kurse</h2>
                <span class="badge text-bg-secondary">{{ enrollments.length }}</span>
              </div>
              <div class="card-body p-0">
                <div v-if="hasEnrollments" class="list-group list-group-flush">
                  <div v-for="enrollment in enrollments" :key="enrollment.id" class="list-group-item py-3">
                    <div class="d-flex flex-column flex-sm-row justify-content-between gap-3">
                      <div>
                        <strong class="d-block">{{ enrollment.course?.title || "Kurs gelöscht" }}</strong>
                        <div class="d-flex flex-wrap gap-2 mt-2">
                          <span class="badge text-bg-light">{{ enrollment.course?.level || "Kein Level" }}</span>
                          <span class="badge text-bg-light">{{ enrollment.course?.category || "Keine Kategorie"
                            }}</span>
                        </div>
                      </div>
                      <button type="button" class="btn btn-outline-primary btn-sm align-self-sm-start"
                        :disabled="downloadingCertificateId === enrollment.courseId"
                        @click="downloadCertificate(enrollment.courseId, enrollment.course?.title)">
                        <span v-if="downloadingCertificateId === enrollment.courseId"
                          class="spinner-border spinner-border-sm me-2"></span>
                        {{ downloadingCertificateId === enrollment.courseId ? "Wird geladen..." : "Zertifikat" }}
                      </button>
                    </div>
                  </div>
                </div>
                <BaseEmptyState v-else title="Noch keine Kurse"
                  message="Deine Kurseinschreibungen werden hier angezeigt." icon="bi-book" />
              </div>
            </section>
          </div>
        </div>

        <section class="card border-0 shadow-sm mt-4">
          <div class="card-header bg-body border-bottom py-3 d-flex justify-content-between align-items-center">
            <h2 class="h5 mb-0">Abgeschlossene Lektionen</h2>
            <span class="badge text-bg-secondary">{{ progress.length }}</span>
          </div>
          <div class="card-body p-0">
            <div v-if="hasProgress" class="list-group list-group-flush">
              <div v-for="item in progress" :key="item.id" class="list-group-item py-3">
                <div class="d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-2">
                  <div class="d-flex align-items-center gap-3">
                    <i class="bi bi-check-circle-fill text-success"></i>
                    <div>
                      <strong class="d-block">Lektion {{ item.lessonId }}</strong>
                      <span class="small text-body-secondary">Abgeschlossen am {{ formatDateTime(item.completedAt)
                        }}</span>
                    </div>
                  </div>
                  <span class="badge text-bg-success">Abgeschlossen</span>
                </div>
              </div>
            </div>
            <BaseEmptyState v-else title="Noch keine Lektionen abgeschlossen"
              message="Sobald du eine Lektion abschließt, erscheint sie hier." icon="bi-check-circle" />
          </div>
        </section>
      </template>
    </template>
  </main>
</template>
