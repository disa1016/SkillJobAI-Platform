import api from "./api";

export const getCandidateDashboard = async () => {
  const { data } = await api.get("/candidate/dashboard");
  return data;
};
export const getMyApplications = async () => {
  const { data } = await api.get("/applications/my");
  return data;
};
export const getProfile = async () => {
  const { data } = await api.get("/users/profile");
  return data;
};

export const uploadCv = async (file) => {
  const formData = new FormData();
  formData.append("file", file);

  const { data } = await api.post("/users/cv", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return data;
};

export const deleteCv = async () => {
  await api.delete("/users/cv");
};

export const getMyEnrollments = async () => {
  const { data } = await api.get("/enrollments/my");
  return data;
};

export const downloadCourseCertificate = async (courseId) => {
  const { data } = await api.get(`/certificates/course/${courseId}`, {
    responseType: "blob",
  });

  return data;
};
export const getMyProgress = async () => {
  const { data } = await api.get("/progress/my");
  return data;
};
export const applyToJob = async ({
  jobId,
  coverLetter,
  cvFile,
  certificateFile,
  portfolioFile,
}) => {
  const formData = new FormData();

  formData.append("jobId", jobId);
  formData.append("coverLetter", coverLetter);

  if (cvFile) formData.append("cvFile", cvFile);
  if (certificateFile) formData.append("certificateFile", certificateFile);
  if (portfolioFile) formData.append("portfolioFile", portfolioFile);

  const { data } = await api.post("/applications", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return data;
};