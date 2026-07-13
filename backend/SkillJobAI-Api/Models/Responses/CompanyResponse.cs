namespace SkillJobAI.Api.Models.Responses;

public class CompanyResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string WebsiteUrl { get; set; } = "";

    public string LogoUrl { get; set; } = "";

    public string Location { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public int TotalJobs { get; set; }

    public List<CompanyJobResponse> Jobs { get; set; } = new();
}

public class CompanyJobResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Location { get; set; } = "";

    public string Salary { get; set; } = "";
}