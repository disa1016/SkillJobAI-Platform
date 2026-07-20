import api from "./api";

/**
 * Lädt die persönlichen Daten des aktuell angemeldeten Benutzers
 * als ZIP-Datei vom Backend herunter.
 */
export const exportPersonalData = async () => {
  const response = await api.get("/users/export", {
    responseType: "blob",
  });

  return {
    blob: response.data,
    fileName: getDownloadFileName(response.headers),
  };
};

/**
 * Löscht das Konto des aktuell angemeldeten Benutzers.
 *
 * Das Backend prüft:
 * - das aktuelle Passwort
 * - die ausdrückliche Löschbestätigung
 * - ob der Benutzer ein Administrator ist
 */
export const deleteAccount = async ({
  password,
  confirmDeletion,
}) => {
  const { data } = await api.delete("/users/account", {
    data: {
      password,
      confirmDeletion,
    },
  });

  return data;
};

/**
 * Liest den Dateinamen aus dem Content-Disposition-Header.
 *
 * Falls das Backend keinen Dateinamen mitsendet,
 * wird ein sinnvoller Standardname verwendet.
 */
const getDownloadFileName = (headers) => {
  const contentDisposition =
    headers?.["content-disposition"] || "";

  /*
   * Unterstützt Header wie:
   *
   * attachment; filename=personal-data.zip
   * attachment; filename="personal-data.zip"
   * attachment; filename*=UTF-8''personal-data.zip
   */

  const utf8FileNameMatch = contentDisposition.match(
    /filename\*=UTF-8''([^;]+)/i,
  );

  if (utf8FileNameMatch?.[1]) {
    return decodeURIComponent(
      utf8FileNameMatch[1].replace(/["']/g, ""),
    );
  }

  const regularFileNameMatch = contentDisposition.match(
    /filename="?([^";]+)"?/i,
  );

  if (regularFileNameMatch?.[1]) {
    return regularFileNameMatch[1].trim();
  }

  const currentDate = new Date()
    .toISOString()
    .slice(0, 10);

  return `skilljobai-personal-data-${currentDate}.zip`;
};

/**
 * Startet den Download einer Blob-Datei im Browser.
 */
export const downloadBlobFile = (blob, fileName) => {
  if (!(blob instanceof Blob)) {
    throw new Error(
      "Die heruntergeladenen Daten sind keine gültige Datei.",
    );
  }

  const downloadUrl = URL.createObjectURL(blob);

  const link = document.createElement("a");

  link.href = downloadUrl;
  link.download = fileName;
  link.style.display = "none";

  document.body.appendChild(link);

  link.click();

  document.body.removeChild(link);

  /*
   * Die temporäre Browser-URL wird nach dem Download
   * wieder freigegeben.
   */
  window.setTimeout(() => {
    URL.revokeObjectURL(downloadUrl);
  }, 1000);
};