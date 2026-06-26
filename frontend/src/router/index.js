import { createRouter, createWebHistory } from "vue-router";

import LandingView from "@/views/LandingView.vue";
import LoginView from "@/views/LoginView.vue";
import RegisterView from "@/views/RegisterView.vue";
import ForgotPasswordView from "@/views/ForgotPasswordView.vue";

import DashboardView from "@/views/DashboardView.vue";
import ProfileView from "@/views/ProfileView.vue";

import CoursesView from "@/views/CoursesView.vue";
import CourseDetailsView from "@/views/CourseDetailsView.vue";

import JobsView from "@/views/JobsView.vue";
import JobDetailsView from "@/views/JobDetailsView.vue";

import CvAnalyzerView from "@/views/CvAnalyzerView.vue";
import JobMatchView from "@/views/JobMatchView.vue";
import JobRecommendationsView from "@/views/JobRecommendationsView.vue";
import CoverLetterView from "@/views/CoverLetterView.vue";

const routes = [
  // Public
  {
    path: "/",
    name: "Landing",
    component: LandingView,
  },
  {
    path: "/home",
    name: "Home",
    component: LandingView,
  },
  {
    path: "/login",
    name: "Login",
    component: LoginView,
  },
  {
    path: "/register",
    name: "Register",
    component: RegisterView,
  },
  {
    path: "/forgot-password",
    name: "ForgotPassword",
    component: ForgotPasswordView,
  },
  {
    path: "/impressum",
    name: "Impressum",
    component: () => import("@/views/ImpressumView.vue"),
  },
  {
    path: "/privacy",
    name: "Privacy",
    component: () => import("@/views/PrivacyView.vue"),
  },
  {
    path: "/contact",
    name: "Contact",
    component: () => import("@/views/ContactView.vue"),
  },

  // Candidate
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
    path: "/profile/skills",
    name: "MySkills",
    component: () => import("@/views/MySkillsView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/my-applications",
    name: "MyApplications",
    component: () => import("@/views/MyApplicationsView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/career-roadmap",
    name: "CareerRoadmap",
    component: () => import("@/views/CareerRoadmapView.vue"),
    meta: { requiresAuth: true },
  },

  // Courses
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

  // Jobs
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
    component: () => import("@/views/SkillGapView.vue"),
    meta: { requiresAuth: true },
  },

  // Companies
  {
    path: "/companies",
    name: "Companies",
    component: () => import("@/views/CompaniesView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/companies/:id",
    name: "CompanyDetail",
    component: () => import("@/views/CompanyDetailView.vue"),
    meta: { requiresAuth: true },
  },

  // AI Tools
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

  // Recruiter
  {
    path: "/recruiter/dashboard",
    name: "RecruiterDashboard",
    component: () => import("@/views/RecruiterDashboard.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/jobs",
    name: "RecruiterJobs",
    component: () => import("@/views/RecruiterJobsView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/jobs/create",
    name: "CreateRecruiterJob",
    component: () => import("@/views/CreateRecruiterJobView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/jobs/edit/:id",
    name: "EditRecruiterJob",
    component: () => import("@/views/EditRecruiterJobView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/applications",
    name: "RecruiterApplications",
    component: () => import("@/views/RecruiterApplicationsView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/applications/:id",
    name: "RecruiterApplicationDetails",
    component: () => import("@/views/RecruiterApplicationDetailsView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/candidates",
    name: "RecruiterCandidates",
    component: () => import("@/views/RecruiterCandidatesView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/candidates/:id",
    name: "RecruiterCandidateDetails",
    component: () => import("@/views/RecruiterCandidateDetailsView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },

  // Admin
  {
    path: "/admin/dashboard",
    name: "AdminDashboard",
    component: () => import("@/views/AdminDashboardView.vue"),
    meta: { requiresAuth: true, roles: ["Admin"] },
  },
  {
    path: "/admin/users",
    name: "AdminUsers",
    component: () => import("@/views/AdminUsersView.vue"),
    meta: { requiresAuth: true, roles: ["Admin"] },
  },
  {
    path: "/admin/companies",
    name: "AdminCompanies",
    component: () => import("@/views/AdminCompaniesView.vue"),
    meta: { requiresAuth: true, roles: ["Admin"] },
  },
  {
    path: "/admin/company-members",
    name: "AdminCompanyMembers",
    component: () => import("@/views/AdminCompanyMembersView.vue"),
    meta: { requiresAuth: true, roles: ["Admin"] },
  },

  // Not Found
  {
    path: "/:pathMatch(.*)*",
    name: "NotFound",
    component: () => import("@/views/NotFoundView.vue"),
  },
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

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token");
  const user = JSON.parse(localStorage.getItem("user") || "null");

  if (to.path === "/" && token) {
    return next(getRedirectPathByRole(user?.role));
  }

  if (to.meta.requiresAuth && !token) {
    return next("/login");
  }

  if (to.meta.roles && !to.meta.roles.includes(user?.role)) {
    return next(getRedirectPathByRole(user?.role));
  }

  if (to.path === "/dashboard" && (user?.role === "Recruiter" || user?.role === "Admin")) {
    return next(getRedirectPathByRole(user.role));
  }

  next();
});

export default router;