using System.ComponentModel.DataAnnotations;
namespace TodoMaster.Models;
public class Category
{
    public int Id{get; set;}
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    public ICollection<Todo> Todos{get; set;}= new List<Todo>();
}