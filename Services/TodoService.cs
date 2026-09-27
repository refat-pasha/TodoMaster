using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;
using TodoMaster.Models;
using TodoMaster.ViewModels;

namespace TodoMaster.Services
{
    public class TodoService : ITodoService
    {
        private readonly AppDbContext _context;

        public TodoService(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Todo/Index
        public async Task<TodoIndexViewModel> GetTodosAsync(
            string? searchTerm = null,
            string? statusFilter = null,
            string? priorityFilter = null,
            int? categoryFilter = null,
            int? tagFilter = null,
            string sortOrder = "created_desc")
        {
            var query = _context.Todos
                .Include(t => t.Category)
                .Include(t => t.Tags)
                .AsNoTracking()
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    (t.Description != null &&
                     t.Description.Contains(searchTerm)));
            }

            // Status filter
            if (statusFilter == "completed")
            {
                query = query.Where(t => t.IsCompleted);
            }
            else if (statusFilter == "pending")
            {
                query = query.Where(t => !t.IsCompleted);
            }
            else if (statusFilter == "overdue")
            {
                query = query.Where(t =>
                    !t.IsCompleted &&
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date < DateTime.Today);
            }

            // Priority filter
            if (!string.IsNullOrWhiteSpace(priorityFilter) &&
                Enum.TryParse<Priority>(
                    priorityFilter,
                    true,
                    out var parsedPriority))
            {
                query = query.Where(t => t.Priority == parsedPriority);
            }

            // Category filter
            if (categoryFilter.HasValue)
            {
                query = query.Where(t =>
                    t.CategoryId == categoryFilter.Value);
            }

            // Tag filter
            if (tagFilter.HasValue)
            {
                query = query.Where(t =>
                    t.Tags.Any(tag => tag.Id == tagFilter.Value));
            }

            // Sorting
            query = sortOrder switch
            {
                "created_asc" =>
                    query.OrderBy(t => t.CreatedAt),

                "title_asc" =>
                    query.OrderBy(t => t.Title),

                "title_desc" =>
                    query.OrderByDescending(t => t.Title),

                "due_asc" =>
                    query.OrderBy(t => t.DueDate),

                "due_desc" =>
                    query.OrderByDescending(t => t.DueDate),

                "priority_asc" =>
                    query.OrderBy(t => t.Priority),

                "priority_desc" =>
                    query.OrderByDescending(t => t.Priority),

                _ =>
                    query.OrderByDescending(t => t.CreatedAt)
            };

            var todos = await query.ToListAsync();

            // Statistics
            var allTodos = await _context.Todos
                .AsNoTracking()
                .ToListAsync();

            var todoViewModels = todos.Select(t => new TodoViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate,
                Priority = t.Priority.ToString(),
                CategoryId = t.CategoryId,
                CategoryName = t.Category?.Name,

                TagNames = t.Tags
                    .Select(tag => tag.Name)
                    .ToList(),

                SelectedTagIds = t.Tags
                    .Select(tag => tag.Id)
                    .ToList()
            }).ToList();

            return new TodoIndexViewModel
            {
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                PriorityFilter = priorityFilter,
                CategoryFilter = categoryFilter,
                TagFilter = tagFilter,
                SortOrder = sortOrder,

                TotalTodos = allTodos.Count,

                CompletedTodos =
                    allTodos.Count(t => t.IsCompleted),

                PendingTodos =
                    allTodos.Count(t => !t.IsCompleted),

                OverdueTodos =
                    allTodos.Count(t =>
                        !t.IsCompleted &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value.Date < DateTime.Today),

                Todos = todoViewModels
            };
        }

        // GET: /Todo/Details/5
        public async Task<TodoViewModel?> GetTodoAsync(int id)
        {
            var todo = await _context.Todos
                .Include(t => t.Category)
                .Include(t => t.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return null;
            }

            return new TodoViewModel
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                CreatedAt = todo.CreatedAt,
                DueDate = todo.DueDate,
                Priority = todo.Priority.ToString(),
                CategoryId = todo.CategoryId,
                CategoryName = todo.Category?.Name,

                TagNames = todo.Tags
                    .Select(tag => tag.Name)
                    .ToList(),

                SelectedTagIds = todo.Tags
                    .Select(tag => tag.Id)
                    .ToList()
            };
        }

        // GET: /Todo/Edit/5
        public async Task<TodoEditViewModel?> GetTodoForEditAsync(int id)
        {
            var todo = await _context.Todos
                .Include(t => t.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return null;
            }

            return new TodoEditViewModel
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                DueDate = todo.DueDate,
                Priority = todo.Priority.ToString(),
                CategoryId = todo.CategoryId,

                SelectedTagIds = todo.Tags
                    .Select(tag => tag.Id)
                    .ToList()
            };
        }

        // POST: /Todo/Create
        public async Task<int> CreateTodoAsync(TodoCreateViewModel model)
        {
            var priority = Priority.Medium;

            if (!string.IsNullOrWhiteSpace(model.Priority))
            {
                Enum.TryParse(
                    model.Priority,
                    true,
                    out priority);
            }

            var todo = new Todo
            {
                Title = model.Title.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(model.Description)
                        ? null
                        : model.Description.Trim(),

                IsCompleted = false,

                CreatedAt = DateTime.Now,

                DueDate = model.DueDate,

                Priority = priority,

                CategoryId = model.CategoryId
            };

            // Add selected Tags
            if (model.SelectedTagIds != null &&
                model.SelectedTagIds.Count > 0)
            {
                var tags = await _context.Tags
                    .Where(t => model.SelectedTagIds.Contains(t.Id))
                    .ToListAsync();

                todo.Tags = tags;
            }

            _context.Todos.Add(todo);

            await _context.SaveChangesAsync();

            return todo.Id;
        }

        // POST: /Todo/Edit/5
        public async Task<bool> UpdateTodoAsync(
            TodoEditViewModel model)
        {
            var todo = await _context.Todos
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (todo == null)
            {
                return false;
            }

            var priority = Priority.Medium;

            if (!string.IsNullOrWhiteSpace(model.Priority))
            {
                Enum.TryParse(
                    model.Priority,
                    true,
                    out priority);
            }

            todo.Title = model.Title.Trim();

            todo.Description =
                string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim();

            todo.IsCompleted = model.IsCompleted;

            todo.DueDate = model.DueDate;

            todo.Priority = priority;

            todo.CategoryId = model.CategoryId;

            // Replace existing Tags
            todo.Tags.Clear();

            if (model.SelectedTagIds != null &&
                model.SelectedTagIds.Count > 0)
            {
                var tags = await _context.Tags
                    .Where(t => model.SelectedTagIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var tag in tags)
                {
                    todo.Tags.Add(tag);
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }

        // POST: /Todo/Delete/5
        public async Task<bool> DeleteTodoAsync(int id)
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return false;
            }

            _context.Todos.Remove(todo);

            await _context.SaveChangesAsync();

            return true;
        }

        // POST: /Todo/ToggleComplete/5
        public async Task<bool> ToggleCompleteAsync(int id)
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return false;
            }

            todo.IsCompleted = !todo.IsCompleted;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}