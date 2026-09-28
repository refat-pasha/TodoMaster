using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;
using TodoMaster.Models;
using TodoMaster.ViewModels;
using TodoMaster.Services;

namespace TodoMaster.Controllers
{
    public class TodoController : Controller
    {
        private readonly ITodoService _todoService;
        private readonly AppDbContext _context;

        public TodoController(
            ITodoService todoService,
            AppDbContext context)
        {
            _todoService = todoService;
            _context = context;
        }


        // =========================================================
        // GET: /Todo
        // Todo list with search, filters, sorting and pagination
        // =========================================================
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? priority,
            int? categoryId,
            int? tagId,
            string? sort,
            int page = 1,
            int pageSize = 10)
        {
            // Allowed page sizes
            var allowedPageSizes = new[] { 5, 10, 20, 50 };

            if (!allowedPageSizes.Contains(pageSize))
            {
                pageSize = 10;
            }

            if (page < 1)
            {
                page = 1;
            }

            var viewModel = await _todoService.GetTodosAsync(
                searchTerm: search,
                statusFilter: status,
                priorityFilter: priority,
                categoryFilter: categoryId,
                tagFilter: tagId,
                sortOrder: sort ?? "created_desc",
                page: page,
                pageSize: pageSize);

            return View(viewModel);
        }


        // =========================================================
        // GET: /Todo/Dashboard
        // =========================================================
        public async Task<IActionResult> Dashboard()
        {
            var todos = await _context.Todos
                .ToListAsync();

            var totalTodos = todos.Count;

            var completedTodos = todos.Count(t =>
                t.IsCompleted);

            var pendingTodos = todos.Count(t =>
                !t.IsCompleted);

            var overdueTodos = todos.Count(t =>
                !t.IsCompleted &&
                t.DueDate.HasValue &&
                t.DueDate.Value.Date < DateTime.Today);

            var todayTodos = todos.Count(t =>
                !t.IsCompleted &&
                t.DueDate.HasValue &&
                t.DueDate.Value.Date == DateTime.Today);

            ViewBag.TotalTodos = totalTodos;
            ViewBag.CompletedTodos = completedTodos;
            ViewBag.PendingTodos = pendingTodos;
            ViewBag.OverdueTodos = overdueTodos;
            ViewBag.TodayTodos = todayTodos;

            return View();
        }


        // =========================================================
        // GET: /Todo/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todos
                .Include(t => t.Category)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }


        // =========================================================
        // GET: /Todo/Create
        // =========================================================
        public async Task<IActionResult> Create()
        {
            await LoadCreateDataAsync();

            return View();
        }


        // =========================================================
        // POST: /Todo/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TodoCreateViewModel model)
        {
            // -----------------------------------------------------
            // Validate Due Date
            // -----------------------------------------------------
            if (model.DueDate.HasValue &&
                model.DueDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.DueDate),
                    "Due date cannot be in the past.");
            }

            // -----------------------------------------------------
            // Validate Priority
            // -----------------------------------------------------
            if (!Enum.IsDefined(typeof(Priority), model.Priority))
            {
                ModelState.AddModelError(
                    nameof(model.Priority),
                    "Please select a valid priority.");
            }

            if (ModelState.IsValid)
            {
                var todoId = await _todoService.CreateTodoAsync(model);

                return RedirectToAction(
                    nameof(Details),
                    new { id = todoId });
            }

            await LoadCreateDataAsync(model);

            return View(model);
        }


        // =========================================================
        // GET: /Todo/Edit/5
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _todoService.GetTodoForEditAsync(id.Value);

            if (model == null)
            {
                return NotFound();
            }

            await LoadEditDataAsync(model);

            return View(model);
        }


        // =========================================================
        // POST: /Todo/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            TodoEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Validate Due Date
            // -----------------------------------------------------
            if (model.DueDate.HasValue &&
                model.DueDate.Value.Date < DateTime.Today &&
                !model.IsCompleted)
            {
                ModelState.AddModelError(
                    nameof(model.DueDate),
                    "An incomplete Todo cannot have a past due date.");
            }

            // -----------------------------------------------------
            // Validate Priority
            // -----------------------------------------------------
            if (!Enum.IsDefined(typeof(Priority), model.Priority))
            {
                ModelState.AddModelError(
                    nameof(model.Priority),
                    "Please select a valid priority.");
            }

            if (ModelState.IsValid)
            {
                var updated = await _todoService.UpdateTodoAsync(model);

                if (!updated)
                {
                    return NotFound();
                }

                return RedirectToAction(
                    nameof(Details),
                    new { id = id });
            }

            await LoadEditDataAsync(model);

            return View(model);
        }


        // =========================================================
        // GET: /Todo/DeleteConfirmation/5
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> DeleteConfirmation(int id)
        {
            var todo = await _context.Todos
                .Include(t => t.Category)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return NotFound();
            }

            return View("Delete", todo);
        }


        // =========================================================
        // POST: /Todo/Delete/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _todoService.DeleteTodoAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // POST: /Todo/ToggleComplete/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var result = await _todoService.ToggleCompleteAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // Load Create View Data
        // =========================================================
        private async Task LoadCreateDataAsync(
            TodoCreateViewModel? model = null)
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var tags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name",
                model?.CategoryId);

            ViewBag.Tags = tags;
        }


        // =========================================================
        // Load Edit View Data
        // =========================================================
        private async Task LoadEditDataAsync(
            TodoEditViewModel model)
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var tags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name",
                model.CategoryId);

            ViewBag.Tags = tags;
        }
    }
}