import api from "./api";

export const getRecruiterDashboard = async () => {
  const { data } = await api.get("/recruiter/dashboard");
  return data;
};

export const getRecruiterCandidates = async (skill = "") => {
  const url = skill
    ? `/recruiter/candidates?skill=${encodeURIComponent(skill)}`
    : "/recruiter/candidates";

  const { data } = await api.get(url);
  return data;
};

export const getRecruiterCandidateById = async (id) => {
  const { data } = await api.get(`/recruiter/candidates/${id}`);
  return data;
};

export const getApplicationsByJob = async (jobId) => {
  const { data } = await api.get(`/applications/job/${jobId}`);
  return data;
};

export const getApplicationById = async (applicationId) => {
  const { data } = await api.get(`/applications/${applicationId}`);
  return data;
};

export const updateApplicationStatus = async (applicationId, status) => {
  const { data } = await api.put(`/applications/${applicationId}/status`, {
    status,
  });

  return data;
};