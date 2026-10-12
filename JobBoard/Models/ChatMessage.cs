using System.ComponentModel.DataAnnotations;

namespace JobBoard.Models;

public class ChatMessage
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser? Sender { get; set; }

    [Required, MaxLength(4000)]
    public string Text { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
