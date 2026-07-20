import { createRouter, createWebHistory } from "vue-router";

import { refreshSession } from "@/services/authService";
// Public
import LandingView from "@/views/public/LandingView.vue";
import ContactView from "@/views/public/ContactView.vue";
import ImpressumView from "@/views/public/ImpressumView.vue";
import PrivacyView from "@/views/public/PrivacyView.vue";
import NotFoundView from "@/views/public/NotFoundView.vue";

// Auth
import LoginView from "@/views/auth/LoginView.vue";
import RegisterView from "@/views/auth/RegisterView.vue";
import ForgotPasswordView from "@/views/auth/ForgotPasswordView.vue";

// Candidate
import DashboardView from "@/views/candidate/DashboardView.vue";
import ProfileView from "@/views/candidate/ProfileView.vue";
import MySkillsView from "@/views/candidate/MySkillsView.vue";
import MyApplicationsView from "@/views/candidate/MyApplicationsView.vue";
import CareerRoadmapView from "@/views/candidate/CareerRoadmapView.vue";

// Account
import AccountSettingsView from "@/views/account/AccountSettingsView.vue";

// Courses
import CoursesView from "@/views/courses/CoursesView.vue";
import CourseDetailsView from "@/views/courses/CourseDetailsView.vue";

// Jobs
import JobsView from "@/views/jobs/JobsView.vue";
import JobDetailsView from "@/views/jobs/JobDetailsView.vue";
import SkillGapView from "@/views/jobs/SkillGapView.vue";
import JobRecommendationsView from "@/views/jobs/JobRecommendationsView.vue";
import CreateRecruiterJobView from "@/views/jobs/CreateRecruiterJobView.vue";
import EditRecruiterJobView from "@/views/jobs/EditRecruiterJobView.vue";

// Companies
import CompaniesView from "@/views/companies/CompaniesView.vue";
import CompanyDetailView from "@/views/companies/CompanyDetailView.vue";

// AI
import CvAnalyzerView from "@/views/ai/CvAnalyzerView.vue";
import JobMatchView from "@/views/ai/JobMatchView.vue";
import CoverLetterView from "@/views/ai/CoverLetterView.vue";

// Recruiter
import RecruiterDashboardView from "@/views/recruiter/RecruiterDashboard.vue";
import RecruiterJobsView from "@/views/recruiter/RecruiterJobsView.vue";
import RecruiterApplicationsView from "@/views/recruiter/RecruiterApplicationsView.vue";
import RecruiterApplicationDetailsView from "@/views/recruiter/RecruiterApplicationDetailsView.vue";
import RecruiterCandidatesView from "@/views/recruiter/RecruiterCandidatesView.vue";
import RecruiterCandidateDetailsView from "@/views/recruiter/RecruiterCandidateDetailsView.vue";

// Admin
import AdminDashboardView from "@/views/admin/AdminDashboardView.vue";
import AdminUsersView from "@/views/admin/AdminUsersView.vue";
import AdminCompaniesView from "@/views/admin/AdminCompaniesView.vue";
import AdminCompanyMembersView from "@/views/admin/AdminCompanyMembersView.vue";

const routes = [
  { path: "/", name: "Landing", component: LandingView },
  { path: "/home", name: "Home", component: LandingView },
  { path: "/impressum", name: "Impressum", component: ImpressumView },
  { path: "/privacy", name: "Privacy", component: PrivacyView },
  { path: "/contact", name: "Contact", component: ContactView },

  { path: "/login", name: "Login", component: LoginView },
  { path: "/register", name: "Register", component: RegisterView },
  {
    path: "/forgot-password",
    name: "ForgotPassword",
    component: ForgotPasswordView,
  },

  {
    path: "/dashboard",
    name: "Dashboard",
    component: DashboardView,
    meta: { requiresAuth: true },
  },
  {
    path: "/profile",
    name: "Profile",
    component: ProfileView,
    meta: { requiresAuth: true },
  },
  {
  path: "/account",
  name: "AccountSettings",
  component: AccountSettingsView,
  meta: { requiresAuth: true },
},
  {
    path: "/profile/skills",
    name: "MySkills",
    component: MySkillsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/my-applications",
    name: "MyApplications",
    component: MyApplicationsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/career-roadmap",
    name: "CareerRoadmap",
    component: CareerRoadmapView,
    meta: { requiresAuth: true },
  },

  {
    path: "/courses",
    name: "Courses",
    component: CoursesView,
    meta: { requiresAuth: true },
  },
  {
    path: "/courses/:id",
    name: "CourseDetails",
    component: CourseDetailsView,
    meta: { requiresAuth: true },
  },

  {
    path: "/jobs",
    name: "Jobs",
    component: JobsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/jobs/:id",
    name: "JobDetails",
    component: JobDetailsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/jobs/:id/skill-gap",
    name: "SkillGap",
    component: SkillGapView,
    meta: { requiresAuth: true },
  },

  {
    path: "/companies",
    name: "Companies",
    component: CompaniesView,
    meta: { requiresAuth: true },
  },
  {
    path: "/companies/:id",
    name: "CompanyDetail",
    component: CompanyDetailView,
    meta: { requiresAuth: true },
  },

  {
    path: "/ai/cv-analyzer",
    name: "CvAnalyzer",
    component: CvAnalyzerView,
    meta: { requiresAuth: true },
  },
  {
    path: "/ai/job-match",
    name: "JobMatch",
    component: JobMatchView,
    meta: { requiresAuth: true },
  },
  {
    path: "/ai/job-recommendations",
    name: "JobRecommendations",
    component: JobRecommendationsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/ai/cover-letter",
    name: "CoverLetter",
    component: CoverLetterView,
    meta: { requiresAuth: true },
  },

  {
    path: "/recruiter/dashboard",
    name: "RecruiterDashboard",
    component: RecruiterDashboardView,
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/jobs",
    name: "RecruiterJobs",
    component: RecruiterJobsView,
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/jobs/create",
    name: "CreateRecruiterJob",
    component: CreateRecruiterJobView,
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/jobs/edit/:id",
    name: "EditRecruiterJob",
    component: EditRecruiterJobView,
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/applications",
    name: "RecruiterApplications",
    component: RecruiterApplicationsView,
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/applications/:id",
    name: "RecruiterApplicationDetails",
    component: RecruiterApplicationDetailsView,
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/candidates",
    name: "RecruiterCandidates",
    component: RecruiterCandidatesView,
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/candidates/:id",
    name: "RecruiterCandidateDetails",
    component: RecruiterCandidateDetailsView,
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },

  {
    path: "/admin/dashboard",
    name: "AdminDashboard",
    component: AdminDashboardView,
    meta: { requiresAuth: true, roles: ["Admin"] },
  },
  {
    path: "/admin/users",
    name: "AdminUsers",
    component: AdminUsersView,
    meta: { requiresAuth: true, roles: ["Admin"] },
  },
  {
    path: "/admin/companies",
    name: "AdminCompanies",
    component: AdminCompaniesView,
    meta: { requiresAuth: true, roles: ["Admin"] },
  },
  {
    path: "/admin/company-members",
    name: "AdminCompanyMembers",
    component: AdminCompanyMembersView,
    meta: { requiresAuth: true, roles: ["Admin"] },
  },

  { path: "/:pathMatch(.*)*", name: "NotFound", component: NotFoundView },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

const getRedirectPathByRole = (role) => {
  if (role === "Admin") return "/admin/dashboard";
  if (role === "Recruiter") return "/recruiter/dashboard";
  return "/dashboard";
};

router.beforeEach(async (to) => {
  let token = localStorage.getItem("token");

  let user = JSON.parse(localStorage.getItem("user") || "null");

  /*
   * Ist kein Access Token vorhanden, kann eventuell
   * noch eine gültige HttpOnly-Cookie-Sitzung bestehen.
   */
  if (!token && to.meta.requiresAuth) {
    try {
      const session = await refreshSession();

      token = session.token;
      user = session.user;
    } catch {
      return {
        path: "/login",
        query: {
          redirect: to.fullPath,
        },
      };
    }
  }

  if (to.path === "/" && token) {
    return getRedirectPathByRole(user?.role);
  }

  if (to.meta.requiresAuth && !token) {
    return {
      path: "/login",
      query: {
        redirect: to.fullPath,
      },
    };
  }

  if (to.meta.roles && !to.meta.roles.includes(user?.role)) {
    return getRedirectPathByRole(user?.role);
  }

  if (
    to.path === "/dashboard" &&
    (user?.role === "Recruiter" || user?.role === "Admin")
  ) {
    return getRedirectPathByRole(user.role);
  }

  return true;
});

export default router;
