using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class TaskUpsertRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [RegularExpression("Low|Medium|High", ErrorMessage = "Priority must be Low, Medium, or High.")]
    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }

    public bool IsCompleted { get; set; }
}
