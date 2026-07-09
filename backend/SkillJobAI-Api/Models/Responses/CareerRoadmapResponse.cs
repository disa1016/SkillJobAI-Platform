namespace SkillJobAI.Api.Models.Responses;

public class CareerRoadmapResponse
{
    public bool HasCareerGoal { get; set; }
    public string? Message { get; set; }

    public CareerRoadmapGoalResponse? Goal { get; set; }

    public int ProgressPercentage { get; set; }
    public int TotalSkills { get; set; }
    public int CompletedSkillsCount { get; set; }
    public int MissingSkillsCount { get; set; }

    public List<CareerRoadmapSkillResponse> CompletedSkills { get; set; } = new();
    public List<CareerRoadmapSkillResponse> MissingSkills { get; set; } = new();
    public List<CareerRoadmapCourseResponse> RecommendedCourses { get; set; } = new();
    public List<CareerRoadmapPhaseResponse> Phases { get; set; } = new();
}

public class CareerRoadmapGoalResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMonths { get; set; }
    public DateTime StartedAt { get; set; }
}

public class CareerRoadmapSkillResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MonthNumber { get; set; }
}

public class CareerRoadmapCourseResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Skill { get; set; } = string.Empty;
}

public class CareerRoadmapPhaseResponse
{
    public int Month { get; set; }
    public List<CareerRoadmapPhaseSkillResponse> Skills { get; set; } = new();
}

public class CareerRoadmapPhaseSkillResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}