using System.ComponentModel.DataAnnotations;
namespace TodoMaster.Models;
using System.ComponentModel.DataAnnotations;


public class Todo
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }
}
public enum Priority
{
    Low,
    Medium,
    High
}