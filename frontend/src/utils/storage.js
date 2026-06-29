export const getCurrentUser = () => {
  return JSON.parse(localStorage.getItem("user") || "null");
};

export const getToken = () => {
  return localStorage.getItem("token");
};

export const clearAuthStorage = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
};