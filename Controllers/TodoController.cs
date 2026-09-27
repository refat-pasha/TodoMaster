using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;
using TodoMaster.Models;
using TodoMaster.Services;
using TodoMaster.ViewModels;

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

        // GET: /Todo/Dashboard
public async Task<IActionResult> Dashboard()
{
    var todos = await _context.Todos
        .Include(t => t.Category)
        .Include(t => t.Tags)
        .ToListAsync();

    var totalTodos = todos.Count;

    var completedTodos = todos.Count(t => t.IsCompleted);

    var pendingTodos = todos.Count(t => !t.IsCompleted);

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

       
       // GET: /Todo
public async Task<IActionResult> Index(
    string? search,
    string? status,
    Priority? priority,
    int? categoryId,
    int? tagId,
    string? sort,
    int page = 1,
    int pageSize = 10)
{
    if (pageSize != 5 &&
        pageSize != 10 &&
        pageSize != 20 &&
        pageSize != 50)
    {
        pageSize = 10;
    }

    var model = await _todoService.GetTodosAsync(
        searchTerm: search,
        statusFilter: status,
        priorityFilter: priority?.ToString(),
        categoryFilter: categoryId,
        tagFilter: tagId,
        sortOrder: sort ?? "created_desc",
        page: page,
        pageSize: pageSize);

    ViewBag.Categories = await _context.Categories
        .OrderBy(c => c.Name)
        .ToListAsync();

    ViewBag.Tags = await _context.Tags
        .OrderBy(t => t.Name)
        .ToListAsync();

    return View(model);
}

        // GET: /Todo/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var todo = await _todoService.GetTodoAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // GET: /Todo/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateDataAsync();

            return View();
        }

        // POST: /Todo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TodoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateDataAsync(model.CategoryId);

                return View(model);
            }

            await _todoService.CreateTodoAsync(model);

            TempData["SuccessMessage"] =
                "Todo created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Todo/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _todoService.GetTodoForEditAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            await LoadEditDataAsync(model.CategoryId);

            return View(model);
        }

        // POST: /Todo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            TodoEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadEditDataAsync(model.CategoryId);

                return View(model);
            }

            var updated = await _todoService.UpdateTodoAsync(model);

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Todo updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Todo/Delete/5
        [HttpGet]
        public async Task<IActionResult> DeleteConfirmation(int id)
        {
            var todo = await _todoService.GetTodoAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return View("Delete", todo);
        }

        // POST: /Todo/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _todoService.DeleteTodoAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Todo deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Todo/ToggleComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var updated =
                await _todoService.ToggleCompleteAsync(id);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // Load Categories and Tags for Create page
        private async Task LoadCreateDataAsync(
            int? selectedCategoryId = null)
        {
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Tags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        // Load Categories and Tags for Edit page
        private async Task LoadEditDataAsync(
            int? selectedCategoryId = null)
        {
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Tags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
    }
}