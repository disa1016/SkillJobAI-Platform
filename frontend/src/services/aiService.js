import api from "./api";

export const generateCoverLetter = async (payload) => {
  const { data } = await api.post("/ai/generate-cover-letter", payload);
  return data;
};