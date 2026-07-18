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

const roleBadgeClass = computed(() => {
  const roleClasses = {
    candidate: "role-candidate",
    student: "role-candidate",
    recruiter: "role-recruiter",
    admin: "role-admin",
  };

  return roleClasses[normalizedRole.value] || "role-default";
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
  <div class="profile-page">
    <div class="container py-4 py-lg-5">
      <div v-if="loading" class="profile-state-card">
        <div class="spinner-border text-success" role="status">
          <span class="visually-hidden">Profil wird geladen...</span>
        </div>

        <h2 class="h5 mt-3 mb-1">Profil wird geladen</h2>

        <p class="text-muted mb-0">
          Deine Profildaten werden vorbereitet.
        </p>
      </div>

      <div v-else-if="error" class="alert alert-danger shadow-sm">
        <div class="d-flex flex-column flex-md-row align-items-md-center gap-3">
          <div class="flex-grow-1">
            <strong>Profil konnte nicht geladen werden.</strong>

            <div class="mt-1">
              {{ error }}
            </div>
          </div>

          <button type="button" class="btn btn-outline-danger" @click="loadProfileData">
            Erneut versuchen
          </button>
        </div>
      </div>

      <template v-else>
        <div v-if="certificateError" class="alert alert-warning shadow-sm">
          {{ certificateError }}
        </div>

        <!-- Profil-Kopfbereich -->
        <section class="profile-hero mb-4">
          <div class="profile-hero-decoration"></div>

          <div class="profile-hero-content">
            <label for="profileImageFile" class="profile-avatar overflow-visible position-relative flex-shrink-0"
              role="button" tabindex="0" title="Profilbild ändern (maximal 2 MB)" aria-label="Profilbild ändern"
              style="width: 128px; height: 128px; cursor: pointer;" @keydown.enter.prevent="profileImageInput?.click()"
              @keydown.space.prevent="profileImageInput?.click()">
              <span class="d-block w-100 h-100 rounded-circle overflow-hidden border border-4 border-white shadow-sm">
                <img v-if="displayedProfileImageUrl" :src="displayedProfileImageUrl"
                  :alt="`Profilbild von ${user?.fullName || 'Benutzer'}`" class="w-100 h-100 object-fit-cover" />
                <span v-else class="w-100 h-100 d-flex align-items-center justify-content-center">
                  {{ userInitials }}
                </span>
              </span>

              <span
                class="position-absolute bottom-0 end-0 d-flex align-items-center justify-content-center rounded-circle bg-white text-success shadow border"
                style="width: 38px; height: 38px; transform: translate(10%, 10%);" aria-hidden="true">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
                  stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M14.5 4h-5L7.8 6H5a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-2.8z" />
                  <circle cx="12" cy="13" r="3" />
                </svg>
              </span>

              <span v-if="uploadingProfileImage"
                class="position-absolute top-50 start-50 translate-middle spinner-border text-light" role="status"
                aria-label="Profilbild wird hochgeladen"></span>
            </label>

            <input id="profileImageFile" ref="profileImageInput" type="file"
              accept="image/jpeg,image/png,.jpg,.jpeg,.png" class="visually-hidden"
              :disabled="uploadingProfileImage || deletingProfileImage" @change="handleProfileImageChange" />

            <div class="profile-identity">
              <div class="d-flex flex-wrap align-items-center gap-2 mb-2">
                <h1 class="profile-name mb-0">
                  {{ user?.fullName || "Unbekannter Benutzer" }}
                </h1>

                <span class="role-badge" :class="roleBadgeClass">
                  {{ roleLabel }}
                </span>
              </div>

              <p class="profile-email mb-2">
                {{ user?.email || "Keine E-Mail hinterlegt" }}
              </p>

              <p v-if="user?.createdAt" class="profile-member-since mb-0">
                Mitglied seit {{ formatDate(user.createdAt) }}
              </p>

              <button v-if="user?.profileImageUrl" type="button"
                class="btn btn-link btn-sm text-white text-decoration-underline p-0 mt-2"
                :disabled="uploadingProfileImage || deletingProfileImage" @click="handleDeleteProfileImage">
                {{ deletingProfileImage ? "Foto wird entfernt..." : "Foto entfernen" }}
              </button>
            </div>

            <div class="profile-completion">
              <div class="completion-header">
                <span>Profilstatus</span>
                <strong>{{ profileCompletion }} %</strong>
              </div>

              <div class="progress" role="progressbar" :aria-valuenow="profileCompletion" aria-valuemin="0"
                aria-valuemax="100">
                <div class="progress-bar" :style="{ width: `${profileCompletion}%` }"></div>
              </div>

              <small>
                {{
                  profileCompletion === 100
                    ? "Dein Profil ist vollständig."
                    : "Vervollständige dein Profil für bessere Ergebnisse."
                }}
              </small>
            </div>
          </div>
        </section>

        <div class="row g-4">
          <!-- Allgemeine Informationen -->
          <div :class="isCandidate ? 'col-lg-5' : 'col-12'">
            <section class="content-card h-100">
              <div class="section-heading">
                <div>
                  <p class="section-eyebrow">
                    Konto
                  </p>

                  <h2 class="section-title">
                    Persönliche Informationen
                  </h2>
                </div>

                <button v-if="!editingProfile" type="button" class="btn btn-outline-success btn-sm"
                  @click="startProfileEditing">
                  Profil bearbeiten
                </button>
              </div>

              <div v-if="profileImageMessage" class="alert alert-success">
                {{ profileImageMessage }}
              </div>

              <div v-if="profileImageError" class="alert alert-danger">
                {{ profileImageError }}
              </div>

              <div v-if="profileMessage" class="alert alert-success">
                {{ profileMessage }}
              </div>

              <div v-if="profileError" class="alert alert-danger">
                {{ profileError }}
              </div>

              <form v-if="editingProfile" @submit.prevent="handleUpdateProfile">
                <div class="mb-3">
                  <label for="profileFullName" class="form-label fw-semibold">
                    Vollständiger Name
                  </label>
                  <input id="profileFullName" v-model="profileForm.fullName" type="text" class="form-control"
                    :class="{ 'is-invalid': getProfileFieldError('fullName') }" autocomplete="name" required />
                  <div class="invalid-feedback">
                    {{ getProfileFieldError("fullName") }}
                  </div>
                </div>

                <div class="mb-3">
                  <label for="profilePhoneNumber" class="form-label fw-semibold">
                    Telefonnummer
                  </label>
                  <input id="profilePhoneNumber" v-model="profileForm.phoneNumber" type="tel" class="form-control"
                    :class="{ 'is-invalid': getProfileFieldError('phoneNumber') }" placeholder="+491234567890"
                    autocomplete="tel" />
                  <div class="invalid-feedback">
                    {{ getProfileFieldError("phoneNumber") }}
                  </div>
                </div>

                <div class="mb-3">
                  <label for="profileLocation" class="form-label fw-semibold">
                    Standort
                  </label>
                  <input id="profileLocation" v-model="profileForm.location" type="text" class="form-control"
                    :class="{ 'is-invalid': getProfileFieldError('location') }" placeholder="Berlin" />
                  <div class="invalid-feedback">
                    {{ getProfileFieldError("location") }}
                  </div>
                </div>

                <div class="mb-3">
                  <label for="profileHeadline" class="form-label fw-semibold">
                    Berufsbezeichnung
                  </label>
                  <input id="profileHeadline" v-model="profileForm.headline" type="text" class="form-control"
                    :class="{ 'is-invalid': getProfileFieldError('headline') }" placeholder="Full-Stack Developer" />
                  <div class="invalid-feedback">
                    {{ getProfileFieldError("headline") }}
                  </div>
                </div>

                <div class="mb-3">
                  <label for="profileAbout" class="form-label fw-semibold">
                    Über mich
                  </label>
                  <textarea id="profileAbout" v-model="profileForm.about" class="form-control"
                    :class="{ 'is-invalid': getProfileFieldError('about') }" rows="5"
                    placeholder="Erzähle etwas über dich..."></textarea>
                  <div class="invalid-feedback">
                    {{ getProfileFieldError("about") }}
                  </div>
                </div>

                <div class="mb-3">
                  <label for="profileLinkedIn" class="form-label fw-semibold">
                    LinkedIn
                  </label>
                  <input id="profileLinkedIn" v-model="profileForm.linkedInUrl" type="url" class="form-control"
                    :class="{ 'is-invalid': getProfileFieldError('linkedInUrl') }"
                    placeholder="https://www.linkedin.com/in/..." />
                  <div class="invalid-feedback">
                    {{ getProfileFieldError("linkedInUrl") }}
                  </div>
                </div>

                <div class="mb-3">
                  <label for="profileGithub" class="form-label fw-semibold">
                    GitHub
                  </label>
                  <input id="profileGithub" v-model="profileForm.githubUrl" type="url" class="form-control"
                    :class="{ 'is-invalid': getProfileFieldError('githubUrl') }" placeholder="https://github.com/..." />
                  <div class="invalid-feedback">
                    {{ getProfileFieldError("githubUrl") }}
                  </div>
                </div>

                <div class="mb-3">
                  <label for="profileWebsite" class="form-label fw-semibold">
                    Website
                  </label>
                  <input id="profileWebsite" v-model="profileForm.website" type="url" class="form-control"
                    :class="{ 'is-invalid': getProfileFieldError('website') }" placeholder="https://example.com" />
                  <div class="invalid-feedback">
                    {{ getProfileFieldError("website") }}
                  </div>
                </div>

                <div class="d-flex flex-column flex-sm-row justify-content-end gap-2">
                  <button type="button" class="btn btn-outline-secondary" :disabled="savingProfile"
                    @click="cancelProfileEditing">
                    Abbrechen
                  </button>

                  <button type="submit" class="btn btn-success" :disabled="savingProfile">
                    <span v-if="savingProfile" class="spinner-border spinner-border-sm me-2"></span>
                    {{ savingProfile ? "Wird gespeichert..." : "Änderungen speichern" }}
                  </button>
                </div>
              </form>

              <div v-else class="information-list">
                <div class="information-item">
                  <div class="information-icon">N</div>
                  <div>
                    <span class="information-label">Vollständiger Name</span>
                    <strong class="information-value">
                      {{ user?.fullName || "Nicht angegeben" }}
                    </strong>
                  </div>
                </div>

                <div class="information-item">
                  <div class="information-icon">@</div>
                  <div>
                    <span class="information-label">E-Mail-Adresse</span>
                    <strong class="information-value">
                      {{ user?.email || "Nicht angegeben" }}
                    </strong>
                  </div>
                </div>

                <div class="information-item">
                  <div class="information-icon">T</div>
                  <div>
                    <span class="information-label">Telefonnummer</span>
                    <strong class="information-value">
                      {{ user?.phoneNumber || "Nicht angegeben" }}
                    </strong>
                  </div>
                </div>

                <div class="information-item">
                  <div class="information-icon">O</div>
                  <div>
                    <span class="information-label">Standort</span>
                    <strong class="information-value">
                      {{ user?.location || "Nicht angegeben" }}
                    </strong>
                  </div>
                </div>

                <div class="information-item">
                  <div class="information-icon">B</div>
                  <div>
                    <span class="information-label">Berufsbezeichnung</span>
                    <strong class="information-value">
                      {{ user?.headline || "Nicht angegeben" }}
                    </strong>
                  </div>
                </div>

                <div v-if="user?.about" class="information-item">
                  <div class="information-icon">I</div>
                  <div>
                    <span class="information-label">Über mich</span>
                    <strong class="information-value">{{ user.about }}</strong>
                  </div>
                </div>

                <div class="information-item">
                  <div class="information-icon">R</div>
                  <div>
                    <span class="information-label">Benutzerrolle</span>
                    <strong class="information-value">{{ roleLabel }}</strong>
                  </div>
                </div>

                <div v-if="user?.linkedInUrl" class="information-item">
                  <div class="information-icon">in</div>
                  <div>
                    <span class="information-label">LinkedIn</span>
                    <a :href="user.linkedInUrl" target="_blank" rel="noopener noreferrer" class="information-value">
                      Profil öffnen
                    </a>
                  </div>
                </div>

                <div v-if="user?.githubUrl" class="information-item">
                  <div class="information-icon">G</div>
                  <div>
                    <span class="information-label">GitHub</span>
                    <a :href="user.githubUrl" target="_blank" rel="noopener noreferrer" class="information-value">
                      Profil öffnen
                    </a>
                  </div>
                </div>

                <div v-if="user?.website" class="information-item">
                  <div class="information-icon">W</div>
                  <div>
                    <span class="information-label">Website</span>
                    <a :href="user.website" target="_blank" rel="noopener noreferrer" class="information-value">
                      Website öffnen
                    </a>
                  </div>
                </div>

                <div v-if="user?.createdAt" class="information-item">
                  <div class="information-icon">D</div>
                  <div>
                    <span class="information-label">Registriert am</span>
                    <strong class="information-value">
                      {{ formatDate(user.createdAt) }}
                    </strong>
                  </div>
                </div>
              </div>
            </section>
          </div>

          <!-- Lebenslauf -->
          <div v-if="isCandidate" class="col-lg-7">
            <section class="content-card h-100">
              <div class="section-heading">
                <div>
                  <p class="section-eyebrow">
                    Bewerbungsunterlagen
                  </p>

                  <h2 class="section-title">
                    Lebenslauf
                  </h2>
                </div>

                <span class="document-status" :class="{ available: user?.cvUrl }">
                  {{ user?.cvUrl ? "Hochgeladen" : "Nicht vorhanden" }}
                </span>
              </div>

              <div v-if="cvMessage" class="alert alert-success">
                {{ cvMessage }}
              </div>

              <div v-if="cvError" class="alert alert-danger">
                {{ cvError }}
              </div>

              <div v-if="user?.cvUrl" class="current-document">
                <div class="document-preview">
                  <div class="pdf-symbol">
                    PDF
                  </div>

                  <div class="flex-grow-1">
                    <strong class="d-block">
                      Dein aktueller Lebenslauf
                    </strong>

                    <span class="text-muted small">
                      Bereit für Bewerbungen
                    </span>
                  </div>
                </div>

                <div class="document-actions">
                  <a :href="cvFullUrl" target="_blank" rel="noopener noreferrer" class="btn btn-outline-success">
                    CV anzeigen
                  </a>

                  <button type="button" class="btn btn-outline-danger" :disabled="deletingCv" @click="handleDeleteCv">
                    <span v-if="deletingCv" class="spinner-border spinner-border-sm me-2"></span>

                    {{ deletingCv ? "Wird gelöscht..." : "CV löschen" }}
                  </button>
                </div>
              </div>

              <div v-else class="empty-document">
                <div class="empty-document-icon">
                  PDF
                </div>

                <div>
                  <strong class="d-block mb-1">
                    Noch kein Lebenslauf vorhanden
                  </strong>

                  <span class="text-muted">
                    Lade einen PDF-Lebenslauf hoch, um dich direkt bewerben
                    zu können.
                  </span>
                </div>
              </div>

              <div class="upload-area mt-4">
                <label for="cvFile" class="form-label fw-semibold">
                  Neuen Lebenslauf auswählen
                </label>

                <input id="cvFile" ref="cvInput" type="file" accept="application/pdf,.pdf" class="form-control"
                  @change="handleCvChange" />

                <div v-if="selectedCvName" class="selected-file mt-3">
                  <span>Ausgewählte Datei:</span>
                  <strong>{{ selectedCvName }}</strong>
                </div>

                <div class="d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 mt-3">
                  <small class="text-muted">
                    PDF-Datei, maximal 5 MB.
                  </small>

                  <button type="button" class="btn btn-success" :disabled="!selectedCv || uploadingCv"
                    @click="handleUploadCv">
                    <span v-if="uploadingCv" class="spinner-border spinner-border-sm me-2"></span>

                    {{
                      uploadingCv
                        ? "Wird hochgeladen..."
                        : "Lebenslauf hochladen"
                    }}
                  </button>
                </div>
              </div>
            </section>
          </div>
        </div>

        <!-- Candidate-Bereich -->
        <template v-if="isCandidate">
          <section class="mt-4">
            <div class="section-page-heading">
              <div>
                <p class="section-eyebrow">
                  Übersicht
                </p>

                <h2 class="section-title">
                  Deine Aktivitäten
                </h2>
              </div>
            </div>

            <div class="row g-4">
              <div class="col-md-4">
                <div class="stat-card">
                  <span class="stat-label">
                    Bewerbungen
                  </span>

                  <strong class="stat-value">
                    {{ applications.length }}
                  </strong>

                  <span class="stat-description">
                    Insgesamt eingereichte Bewerbungen
                  </span>
                </div>
              </div>

              <div class="col-md-4">
                <div class="stat-card">
                  <span class="stat-label">
                    Kurse
                  </span>

                  <strong class="stat-value">
                    {{ enrollments.length }}
                  </strong>

                  <span class="stat-description">
                    Aktuelle Kurseinschreibungen
                  </span>
                </div>
              </div>

              <div class="col-md-4">
                <div class="stat-card">
                  <span class="stat-label">
                    Lektionen
                  </span>

                  <strong class="stat-value">
                    {{ progress.length }}
                  </strong>

                  <span class="stat-description">
                    Erfolgreich abgeschlossene Lektionen
                  </span>
                </div>
              </div>
            </div>
          </section>

          <div class="row g-4 mt-1">
            <!-- Bewerbungen -->
            <div class="col-lg-6">
              <section class="content-card h-100">
                <div class="section-heading">
                  <div>
                    <p class="section-eyebrow">
                      Jobs
                    </p>

                    <h2 class="section-title">
                      Meine Bewerbungen
                    </h2>
                  </div>

                  <span class="count-badge">
                    {{ applications.length }}
                  </span>
                </div>

                <div v-if="hasApplications" class="activity-list">
                  <article v-for="application in applications" :key="application.id" class="activity-item">
                    <div class="activity-avatar">
                      {{
                        application.job?.company
                          ?.charAt(0)
                          ?.toUpperCase() || "J"
                      }}
                    </div>

                    <div class="activity-content">
                      <strong class="activity-title">
                        {{ application.job?.title || "Job gelöscht" }}
                      </strong>

                      <span class="activity-meta">
                        {{ application.job?.company || "Keine Firma" }}
                      </span>
                    </div>

                    <span class="status-pill">
                      {{ application.status || "Unbekannt" }}
                    </span>
                  </article>
                </div>

                <div v-else class="empty-state">
                  <div class="empty-state-symbol">
                    J
                  </div>

                  <strong>Noch keine Bewerbungen</strong>

                  <p>
                    Deine eingereichten Bewerbungen werden hier angezeigt.
                  </p>
                </div>
              </section>
            </div>

            <!-- Kurse -->
            <div class="col-lg-6">
              <section class="content-card h-100">
                <div class="section-heading">
                  <div>
                    <p class="section-eyebrow">
                      Weiterbildung
                    </p>

                    <h2 class="section-title">
                      Meine Kurse
                    </h2>
                  </div>

                  <span class="count-badge">
                    {{ enrollments.length }}
                  </span>
                </div>

                <div v-if="hasEnrollments" class="activity-list">
                  <article v-for="enrollment in enrollments" :key="enrollment.id" class="course-item">
                    <div class="d-flex align-items-start gap-3">
                      <div class="activity-avatar">
                        K
                      </div>

                      <div class="flex-grow-1">
                        <strong class="activity-title d-block">
                          {{ enrollment.course?.title || "Kurs gelöscht" }}
                        </strong>

                        <div class="course-tags">
                          <span>
                            {{ enrollment.course?.level || "Kein Level" }}
                          </span>

                          <span>
                            {{
                              enrollment.course?.category ||
                              "Keine Kategorie"
                            }}
                          </span>
                        </div>
                      </div>
                    </div>

                    <button type="button" class="btn btn-outline-success btn-sm mt-3" :disabled="downloadingCertificateId === enrollment.courseId
                      " @click="
                        downloadCertificate(
                          enrollment.courseId,
                          enrollment.course?.title
                        )
                        ">
                      <span v-if="
                        downloadingCertificateId === enrollment.courseId
                      " class="spinner-border spinner-border-sm me-2"></span>

                      {{
                        downloadingCertificateId === enrollment.courseId
                          ? "Wird geladen..."
                          : "Zertifikat herunterladen"
                      }}
                    </button>
                  </article>
                </div>

                <div v-else class="empty-state">
                  <div class="empty-state-symbol">
                    K
                  </div>

                  <strong>Noch keine Kurse</strong>

                  <p>
                    Deine Kurseinschreibungen werden hier angezeigt.
                  </p>
                </div>
              </section>
            </div>
          </div>

          <!-- Lernfortschritt -->
          <section class="content-card mt-4">
            <div class="section-heading">
              <div>
                <p class="section-eyebrow">
                  Lernfortschritt
                </p>

                <h2 class="section-title">
                  Abgeschlossene Lektionen
                </h2>
              </div>

              <span class="count-badge">
                {{ progress.length }}
              </span>
            </div>

            <div v-if="hasProgress" class="progress-list">
              <div v-for="item in progress" :key="item.id" class="progress-item">
                <div class="progress-check">
                  ✓
                </div>

                <div class="flex-grow-1">
                  <strong class="d-block">
                    Lektion {{ item.lessonId }}
                  </strong>

                  <span class="text-muted small">
                    Abgeschlossen am
                    {{ formatDateTime(item.completedAt) }}
                  </span>
                </div>

                <span class="completed-label">
                  Abgeschlossen
                </span>
              </div>
            </div>

            <div v-else class="empty-state compact">
              <div class="empty-state-symbol">
                ✓
              </div>

              <strong>Noch keine Lektionen abgeschlossen</strong>

              <p>
                Sobald du eine Lektion abschließt, erscheint sie hier.
              </p>
            </div>
          </section>
        </template>
      </template>
    </div>
  </div>
</template>
