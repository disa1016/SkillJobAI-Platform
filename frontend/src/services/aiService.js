import api from "./api";

export const generateCoverLetter = async (payload) => {
  const { data } = await api.post("/ai/generate-cover-letter", payload);
  return data;
};

export const analyzeCv = async (cvText) => {
  const { data } = await api.post("/ai/analyze-cv", {
    cvText,
  });

  return data;
};

export const analyzeCvPdf = async (file) => {
  const formData = new FormData();
  formData.append("file", file);

  const { data } = await api.post("/ai/analyze-cv-pdf", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return data;
};

export const analyzeJobMatch = async (payload) => {
  const { data } = await api.post("/ai/job-match", payload);
  return data;
};
