using System.ComponentModel.DataAnnotations;

namespace TodoMaster.ViewModels
{
    public class TodoEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        public string Priority { get; set; } = "Medium";

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public List<int> SelectedTagIds { get; set; } = new();
    }
}