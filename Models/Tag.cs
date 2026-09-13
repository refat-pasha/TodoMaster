using System.ComponentModel.DataAnnotations;

namespace TodoMaster.Models;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
}