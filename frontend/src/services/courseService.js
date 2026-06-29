import api from "./api";

export const getCourses = async ({ page = 1, pageSize = 10, search = "" } = {}) => {
  const { data } = await api.get("/courses", {
    params: {
      page,
      pageSize,
      search,
    },
  });

  return data;
};

export const getCourseById = async (id) => {
  const { data } = await api.get(`/courses/${id}`);
  return data;
};

export const enrollInCourse = async (courseId) => {
  const { data } = await api.post("/enrollments", {
    courseId,
  });

  return data;
};

export const getMyProgress = async () => {
  const { data } = await api.get("/progress/my");
  return data;
};

export const completeLessonById = async (lessonId) => {
  const { data } = await api.post("/progress/complete", {
    lessonId,
  });

  return data;
};