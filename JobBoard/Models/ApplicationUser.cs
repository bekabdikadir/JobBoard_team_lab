using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace JobBoard.Models;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(150)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? PhotoPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string AvatarUrl => string.IsNullOrEmpty(PhotoPath) ? "/images/default-avatar.svg" : PhotoPath;
}
