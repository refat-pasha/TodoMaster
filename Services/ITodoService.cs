using TodoMaster.ViewModels;

namespace TodoMaster.Services
{
    public interface ITodoService
    {
        Task<TodoIndexViewModel> GetTodosAsync(
            string? searchTerm = null,
            string? statusFilter = null,
            string? priorityFilter = null,
            int? categoryFilter = null,
            int? tagFilter = null,
            string sortOrder = "created_desc");

        Task<TodoEditViewModel?> GetTodoForEditAsync(int id);

        Task<TodoViewModel?> GetTodoAsync(int id);

        Task<int> CreateTodoAsync(TodoCreateViewModel model);

        Task<bool> UpdateTodoAsync(TodoEditViewModel model);

        Task<bool> DeleteTodoAsync(int id);

        Task<bool> ToggleCompleteAsync(int id);
    }
}