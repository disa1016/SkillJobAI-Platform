export const getMatchClass = (score) => {
  if (score >= 70) return "bg-success";
  if (score >= 40) return "bg-warning text-dark";

  return "bg-danger";
};