import api from "./api";

export const getAdminDashboard = async () => {
  const { data } = await api.get("/admin/dashboard");
  return data;
};

export const getAdminUsers = async () => {
  const { data } = await api.get("/admin/users");
  return data;
};

export const getCompanyMembers = async () => {
  const { data } = await api.get("/company-members");
  return data;
};

export const assignCompanyMember = async (companyId, userId) => {
  const { data } = await api.post("/company-members", {
    companyId,
    userId,
    role: "Recruiter",
  });

  return data;
};

export const removeCompanyMember = async (memberId) => {
  const { data } = await api.delete(`/company-members/${memberId}`);
  return data;
};

export const updateUserRole = async (userId, role) => {
  const { data } = await api.put(`/admin/users/${userId}/role`, {
    role,
  });

  return data;
};

export const deleteAdminUser = async (userId) => {
  await api.delete(`/admin/users/${userId}`);
};