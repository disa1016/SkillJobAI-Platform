namespace SkillJobAI.Api.Models.Responses;

public class AdminDashboardResponse
{
    public int TotalUsers { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalJobs { get; set; }
    public int TotalApplications { get; set; }
    public int TotalCourses { get; set; }
    public int TotalSkills { get; set; }

    public int NewUsersToday { get; set; }
    public int NewApplicationsToday { get; set; }

    public int TotalRecruiters { get; set; }
    public int TotalAdmins { get; set; }
}