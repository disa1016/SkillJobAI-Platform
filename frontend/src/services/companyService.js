import api from "./api";

export const getCompanies = async ({ page = 1, pageSize = 10, search = "" } = {}) => {
  const { data } = await api.get("/companies", {
    params: {
      page,
      pageSize,
      search,
    },
  });

  return data;
};

export const getCompanyById = async (id) => {
  const { data } = await api.get(`/companies/${id}`);
  return data;
};

export const createCompany = async (company) => {
  const { data } = await api.post("/companies", company);
  return data;
};

export const updateCompany = async (id, company) => {
  const { data } = await api.put(`/companies/${id}`, company);
  return data;
};

export const deleteCompany = async (id) => {
  await api.delete(`/companies/${id}`);
};

export const uploadCompanyLogo = async (companyId, file) => {
  const formData = new FormData();
  formData.append("file", file);

  const { data } = await api.post(`/companies/${companyId}/logo`, formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return data;
};