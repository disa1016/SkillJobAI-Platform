import api from "./api";

export const getJobs = async () => {
  const { data } = await api.get("/jobs");
  return data;
};

export const getJobById = async (id) => {
  const { data } = await api.get(`/jobs/${id}`);
  return data;
};

export const createJob = async (job) => {
  const { data } = await api.post("/jobs", job);
  return data;
};

export const updateJob = async (id, job) => {
  const { data } = await api.put(`/jobs/${id}`, job);
  return data;
};

export const deleteJob = async (id) => {
  await api.delete(`/jobs/${id}`);
};

export const getJobSkills = async (jobId) => {
  const { data } = await api.get(`/jobs/${jobId}/skills`);
  return data;
};

export const addSkillToJob = async (jobId, skillId) => {
  const { data } = await api.post(`/jobs/${jobId}/skills/${skillId}`);
  return data;
};

export const removeSkillFromJob = async (jobId, skillId) => {
  await api.delete(`/jobs/${jobId}/skills/${skillId}`);
};

export const getSkillGap = async (jobId) => {
  const { data } = await api.get(`/jobs/${jobId}/skill-gap`);
  return data;
};