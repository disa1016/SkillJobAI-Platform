import api, {
  clearLocalSession,
  saveSession,
} from "./api";

export const login = async (credentials) => {
  const { data } =
    await api.post("/auth/login", credentials);

  saveSession(data);

  return data;
};

export const register = async (payload) => {
  const { data } =
    await api.post("/auth/register", payload);

  /*
   * Das Backend meldet einen neuen Benutzer direkt an.
   * Deshalb speichern wir auch hier Access Token und User.
   */
  saveSession(data);

  return data;
};

export const refreshSession = async () => {
  const { data } =
    await api.post("/auth/refresh");

  saveSession(data);

  return data;
};

export const logout = async () => {
  try {
    /*
     * Das Refresh-Cookie wird automatisch vom Browser
     * gesendet und vom Backend widerrufen.
     */
    await api.post("/auth/logout");
  } finally {
    /*
     * Lokale Daten werden auch entfernt, wenn das Backend
     * vorübergehend nicht erreichbar ist.
     */
    clearLocalSession();
  }
};

export const forgotPassword = async (payload) => {
  const { data } =
    await api.post(
      "/auth/forgot-password",
      payload
    );

  return data;
};

export const resetPassword = async (payload) => {
  const { data } =
    await api.post(
      "/auth/reset-password",
      payload
    );

  return data;
};