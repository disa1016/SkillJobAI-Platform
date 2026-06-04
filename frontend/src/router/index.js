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
  {
    path: "/",
    redirect: "/login",
  },
  {
    path: "/login",
    component: LoginView,
  },
  {
    path: "/register",
    component: RegisterView,
  },
  {
    path: "/forgot-password",
    component: ForgotPasswordView,
  },
  {
    path: "/dashboard",
    component: DashboardView,
  },
  {
    path: "/courses",
    component: CoursesView,
  },
  {
    path: "/jobs",
    component: JobsView,
  },

  {
    path: "/profile",
    component: ProfileView,
  },
  {
    path: "/courses/:id",
    component: CourseDetailsView,
  },
  {
    path: "/jobs/:id",
    component: JobDetailsView,
  },
  {
    path: "/ai/cv-analyzer",
    component: CvAnalyzerView,
  },
  {
    path: "/ai/job-match",
    component: JobMatchView,
  },
  {
    path: "/ai/job-recommendations",
    component: JobRecommendationsView,
  },
  {
    path: "/ai/cover-letter",
    component: CoverLetterView,
  },
  {
    path: "/recruiter/dashboard",
    name: "RecruiterDashboard",
    component: () => import("../views/RecruiterDashboard.vue"),
  },
  {
  path: "/jobs/:id/skill-gap",
  name: "SkillGap",
  component: () => import("../views/SkillGapView.vue"),
},
{
  path: "/profile/skills",
  name: "MySkills",
  component: () => import("../views/MySkillsView.vue"),
},
{
  path: "/recruiter/applications",
  name: "RecruiterApplications",
  component: () => import("../views/RecruiterApplicationsView.vue"),
},
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
