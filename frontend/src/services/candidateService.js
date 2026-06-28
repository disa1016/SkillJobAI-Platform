import api from "./api";

export const getCandidateDashboard = async () => {
  const { data } = await api.get("/candidate/dashboard");
  return data;
};