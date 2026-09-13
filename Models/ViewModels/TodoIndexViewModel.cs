namespace TodoMaster.ViewModels
{
    public class TodoIndexViewModel
    {
        public List<TodoViewModel> Todos { get; set; } = new();

        public string? SearchTerm { get; set; }

        public string? StatusFilter { get; set; }

        public string? PriorityFilter { get; set; }

        public int? CategoryFilter { get; set; }

        public int? TagFilter { get; set; }

        public string SortOrder { get; set; } = "created_desc";

        public int TotalTodos { get; set; }

        public int CompletedTodos { get; set; }

        public int PendingTodos { get; set; }

        public int OverdueTodos { get; set; }
    }
}