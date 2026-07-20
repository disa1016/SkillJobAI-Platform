import axios from "axios";

const apiBaseUrl =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5226/api";

/*
 * Regulärer API-Client.
 *
 * withCredentials ist notwendig, damit der Browser
 * das HttpOnly-Refresh-Cookie mitsendet.
 */
const api = axios.create({
  baseURL: apiBaseUrl,
  withCredentials: true,
});

/*
 * Separater Client für Refresh-Anfragen.
 *
 * Dadurch läuft die Refresh-Anfrage nicht selbst
 * erneut durch den Response-Interceptor.
 */
const refreshClient = axios.create({
  baseURL: apiBaseUrl,
  withCredentials: true,
});

let refreshPromise = null;

const clearLocalSession = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
};

const saveSession = (data) => {
  if (data?.token) {
    localStorage.setItem("token", data.token);
  }

  if (data?.user) {
    localStorage.setItem("user", JSON.stringify(data.user));
  }
};

const redirectToLogin = () => {
  const currentPath = window.location.pathname + window.location.search;

  if (window.location.pathname === "/login") {
    return;
  }

  const redirectUrl = `/login?redirect=${encodeURIComponent(currentPath)}`;

  window.location.replace(redirectUrl);
};

const refreshAccessToken = async () => {
  /*
   * Wenn mehrere API-Anfragen gleichzeitig 401 erhalten,
   * wird nur eine einzige Refresh-Anfrage ausgeführt.
   */
  if (!refreshPromise) {
    refreshPromise = refreshClient
      .post("/auth/refresh")
      .then((response) => {
        saveSession(response.data);
        return response.data.token;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
};

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error),
);

api.interceptors.response.use(
  (response) => response,

  async (error) => {
    const originalRequest = error.config;

    const status = error.response?.status;

    const requestUrl = originalRequest?.url || "";

    const isAuthEndpoint =
      requestUrl.includes("/auth/login") ||
      requestUrl.includes("/auth/register") ||
      requestUrl.includes("/auth/refresh") ||
      requestUrl.includes("/auth/logout") ||
      requestUrl.includes("/auth/forgot-password") ||
      requestUrl.includes("/auth/reset-password");

    /*
     * Nur bei einem 401 versuchen wir eine Erneuerung.
     * Auth-Endpunkte selbst dürfen keinen Refresh-Loop auslösen.
     */
    if (
      status !== 401 ||
      !originalRequest ||
      originalRequest._retry ||
      isAuthEndpoint
    ) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      const newAccessToken = await refreshAccessToken();

      originalRequest.headers = originalRequest.headers || {};

      originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;

      return api(originalRequest);
    } catch (refreshError) {
      clearLocalSession();
      redirectToLogin();

      return Promise.reject(refreshError);
    }
  },
);

export { clearLocalSession, saveSession, refreshAccessToken };

export default api;
