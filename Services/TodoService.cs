using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;
using TodoMaster.Models;
using TodoMaster.ViewModels;

namespace TodoMaster.Services
{
    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;

        public TodoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Todo list with search, filters, sorting, category and tag information
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
                .Include(t => t.TodoTags)
                    .ThenInclude(tt => tt.Tag)
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
            if (!string.IsNullOrWhiteSpace(priorityFilter))
            {
                query = query.Where(t => t.Priority == priorityFilter);
            }

            // Category filter
            if (categoryFilter.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryFilter.Value);
            }

            // Tag filter
            if (tagFilter.HasValue)
            {
                query = query.Where(t =>
                    t.TodoTags.Any(tt => tt.TagId == tagFilter.Value));
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

            var allTodos = await _context.Todos
                .AsNoTracking()
                .ToListAsync();

            var model = new TodoIndexViewModel
            {
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                PriorityFilter = priorityFilter,
                CategoryFilter = categoryFilter,
                TagFilter = tagFilter,
                SortOrder = sortOrder,

                TotalTodos = allTodos.Count,

                CompletedTodos = allTodos.Count(t => t.IsCompleted),

                PendingTodos = allTodos.Count(t => !t.IsCompleted),

                OverdueTodos = allTodos.Count(t =>
                    !t.IsCompleted &&
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date < DateTime.Today),

                Todos = todos.Select(t => new TodoViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category?.Name,
                    TagNames = t.TodoTags
                        .Where(tt => tt.Tag != null)
                        .Select(tt => tt.Tag!.Name)
                        .ToList(),
                    SelectedTagIds = t.TodoTags
                        .Select(tt => tt.TagId)
                        .ToList()
                }).ToList()
            };

            return model;
        }

        // GET: /Todo/Details/5
        public async Task<TodoViewModel?> GetTodoAsync(int id)
        {
            var todo = await _context.Todos
                .Include(t => t.Category)
                .Include(t => t.TodoTags)
                    .ThenInclude(tt => tt.Tag)
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
                Priority = todo.Priority,
                CategoryId = todo.CategoryId,
                CategoryName = todo.Category?.Name,
                TagNames = todo.TodoTags
                    .Where(tt => tt.Tag != null)
                    .Select(tt => tt.Tag!.Name)
                    .ToList(),
                SelectedTagIds = todo.TodoTags
                    .Select(tt => tt.TagId)
                    .ToList()
            };
        }

        // GET: /Todo/Edit/5
        public async Task<TodoEditViewModel?> GetTodoForEditAsync(int id)
        {
            var todo = await _context.Todos
                .Include(t => t.TodoTags)
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
                Priority = todo.Priority,
                CategoryId = todo.CategoryId,
                SelectedTagIds = todo.TodoTags
                    .Select(tt => tt.TagId)
                    .ToList()
            };
        }

        // POST: /Todo/Create
        public async Task<int> CreateTodoAsync(TodoCreateViewModel model)
        {
            var todo = new Todo
            {
                Title = model.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),
                IsCompleted = false,
                CreatedAt = DateTime.Now,
                DueDate = model.DueDate,
                Priority = string.IsNullOrWhiteSpace(model.Priority)
                    ? "Medium"
                    : model.Priority,
                CategoryId = model.CategoryId
            };

            _context.Todos.Add(todo);

            await _context.SaveChangesAsync();

            // Add selected tags
            if (model.SelectedTagIds != null &&
                model.SelectedTagIds.Count > 0)
            {
                foreach (var tagId in model.SelectedTagIds.Distinct())
                {
                    var tagExists = await _context.Tags
                        .AnyAsync(t => t.Id == tagId);

                    if (tagExists)
                    {
                        _context.TodoTags.Add(new TodoTag
                        {
                            TodoId = todo.Id,
                            TagId = tagId
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return todo.Id;
        }

        // POST: /Todo/Edit/5
        public async Task<bool> UpdateTodoAsync(TodoEditViewModel model)
        {
            var todo = await _context.Todos
                .Include(t => t.TodoTags)
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (todo == null)
            {
                return false;
            }

            todo.Title = model.Title.Trim();

            todo.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();

            todo.IsCompleted = model.IsCompleted;
            todo.DueDate = model.DueDate;

            todo.Priority = string.IsNullOrWhiteSpace(model.Priority)
                ? "Medium"
                : model.Priority;

            todo.CategoryId = model.CategoryId;

            // Remove existing tags
            _context.TodoTags.RemoveRange(todo.TodoTags);

            // Add selected tags
            if (model.SelectedTagIds != null &&
                model.SelectedTagIds.Count > 0)
            {
                foreach (var tagId in model.SelectedTagIds.Distinct())
                {
                    var tagExists = await _context.Tags
                        .AnyAsync(t => t.Id == tagId);

                    if (tagExists)
                    {
                        todo.TodoTags.Add(new TodoTag
                        {
                            TodoId = todo.Id,
                            TagId = tagId
                        });
                    }
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