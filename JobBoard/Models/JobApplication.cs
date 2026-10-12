namespace JobBoard.Models;

public class JobApplication
{
    public int Id { get; set; }

    public int VacancyId { get; set; }
    public Vacancy? Vacancy { get; set; }

    public int ResumeId { get; set; }
    public Resume? Resume { get; set; }

    public string SeekerId { get; set; } = string.Empty;
    public ApplicationUser? Seeker { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ChatMessage> Messages { get; set; } = new();
}
