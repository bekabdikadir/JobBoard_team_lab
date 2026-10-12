using System.ComponentModel.DataAnnotations;

namespace JobBoard.Models;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
}
