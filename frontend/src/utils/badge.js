export const getStatusBadgeClass = (status) => {
  if (status === "Accepted") return "bg-success";
  if (status === "Rejected") return "bg-danger";
  if (status === "Reviewed") return "bg-info text-dark";

  return "bg-warning text-dark";
};

export const getMatchBadgeClass = (score) => {
  if (score >= 70) return "bg-success";
  if (score >= 40) return "bg-warning text-dark";

  return "bg-danger";
};