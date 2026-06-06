import { createRouter, createWebHistory } from "vue-router";

import LoginView from "../views/LoginView.vue";
import RegisterView from "@/views/RegisterView.vue";
import ForgotPasswordView from "@/views/ForgotPasswordView.vue";

import DashboardView from "../views/DashboardView.vue";
import CvAnalyzerView from "@/views/CvAnalyzerView.vue";

import CoursesView from "@/views/CoursesView.vue";
import JobsView from "@/views/JobsView.vue";
import ProfileView from "@/views/ProfileView.vue";
import CoverLetterView from "@/views/CoverLetterView.vue";

import CourseDetailsView from "@/views/CourseDetailsView.vue";
import JobDetailsView from "@/views/JobDetailsView.vue";


import JobMatchView from "../views/JobMatchView.vue";
import JobRecommendationsView from "@/views/JobRecommendationsView.vue";

const routes = [
  { path: "/", redirect: "/login" },
  { path: "/login", component: LoginView },
  { path: "/register", component: RegisterView },
  { path: "/forgot-password", component: ForgotPasswordView },

  {
    path: "/dashboard",
    component: DashboardView,
    meta: { requiresAuth: true },
  },
  { path: "/courses", component: CoursesView, meta: { requiresAuth: true } },
  { path: "/jobs", component: JobsView, meta: { requiresAuth: true } },
  { path: "/profile", component: ProfileView, meta: { requiresAuth: true } },
  {
    path: "/courses/:id",
    component: CourseDetailsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/jobs/:id",
    component: JobDetailsView,
    meta: { requiresAuth: true },
  },

  {
    path: "/ai/cv-analyzer",
    component: CvAnalyzerView,
    meta: { requiresAuth: true },
  },
  {
    path: "/ai/job-match",
    component: JobMatchView,
    meta: { requiresAuth: true },
  },
  {
    path: "/ai/job-recommendations",
    component: JobRecommendationsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/ai/cover-letter",
    component: CoverLetterView,
    meta: { requiresAuth: true },
  },

  {
    path: "/recruiter/dashboard",
    name: "RecruiterDashboard",
    component: () => import("../views/RecruiterDashboard.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/applications",
    name: "RecruiterApplications",
    component: () => import("../views/RecruiterApplicationsView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/jobs/:id/skill-gap",
    name: "SkillGap",
    component: () => import("../views/SkillGapView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/profile/skills",
    name: "MySkills",
    component: () => import("../views/MySkillsView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/my-applications",
    name: "MyApplications",
    component: () => import("../views/MyApplicationsView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/recruiter/jobs",
    name: "RecruiterJobs",
    component: () => import("../views/RecruiterJobsView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },

  {
    path: "/recruiter/jobs/create",
    name: "CreateRecruiterJob",
    component: () => import("../views/CreateRecruiterJobView.vue"),
    meta: { requiresAuth: true, roles: ["Recruiter", "Admin"] },
  },
  {
    path: "/recruiter/jobs/edit/:id",
    name: "EditRecruiterJob",
    component: () => import("../views/EditRecruiterJobView.vue"),
    meta: {
      requiresAuth: true,
      roles: ["Recruiter", "Admin"],
    },
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token");
  const user = JSON.parse(localStorage.getItem("user") || "null");

  if (to.meta.requiresAuth && !token) {
    return next("/login");
  }

  if (to.meta.roles && !to.meta.roles.includes(user?.role)) {
    return next("/dashboard");
  }

  next();
});

export default router;
