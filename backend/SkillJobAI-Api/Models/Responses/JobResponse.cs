namespace SkillJobAI.Api.Models.Responses;

public class JobResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Location { get; set; } = "";

    public string Salary { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public int? CompanyId { get; set; }

    public string CompanyName { get; set; } = "";

    public JobCompanyResponse? Company { get; set; }
}

public class JobCompanyResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string WebsiteUrl { get; set; } = "";

    public string LogoUrl { get; set; } = "";

    public string Location { get; set; } = "";
}