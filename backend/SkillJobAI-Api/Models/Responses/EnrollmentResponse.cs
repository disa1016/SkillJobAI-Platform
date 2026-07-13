namespace SkillJobAI.Api.Models.Responses;

public class EnrollmentResponse
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public bool IsCompleted { get; set; }

    public EnrollmentCourseResponse? Course { get; set; }
}

public class EnrollmentCourseResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Category { get; set; } = "";

    public string Level { get; set; } = "";

    public string Instructor { get; set; } = "";
}