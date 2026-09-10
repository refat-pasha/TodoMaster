using System.ComponentModel.DataAnnotations;
namespace TodoMaster.Models;
public class Todo
{
    public int Id{get; set;}
    
    [Required]
    [StringLength(100)]
    public string Title{get; set;} = string.Empty;

    [StringLength(500)]
    public string? Description{get; set;}
    public bool IsCompleted{get; set;} 
    public DateTime CreatedAt{get; set;} = DateTime.UtcNow;
    public DateTime? DueDate{get; set;}
    public Priority Priority{get; set;} = Priority.Medium;
}
public enum Priority
{
    Low,
    Medium,
    High
}