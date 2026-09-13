using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TodoMaster.Data;
using TodoMaster.Models;

namespace TodoMaster.Controllers;
public class TodoController: Controller
{
    private readonly AppDbContext _context;
    public TodoController(AppDbContext context)
    {
        _context=context;
    }
   // GET: /Todo
public async Task<IActionResult> Index(
    string? search,
    string? status,
    Priority? priority,
    int? categoryId,
    int? tagId,
    string? sort)
{
    var query = _context.Todos
    .Include(t => t.Category)
    .Include(t => t.Tags)
    .AsQueryable();

    // Search
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(t =>
            t.Title.Contains(search) ||
            (t.Description != null &&
             t.Description.Contains(search)));
    }

    // Status filter
    if (status == "completed")
    {
        query = query.Where(t => t.IsCompleted);
    }
    else if (status == "pending")
    {
        query = query.Where(t => !t.IsCompleted);
    }

    // Priority filter
    if (priority.HasValue)
    {
        query = query.Where(t => t.Priority == priority.Value);
    }

    // Category filter
    if (categoryId.HasValue)
    {
        query = query.Where(t => t.CategoryId == categoryId.Value);
    }
    // Tag filter
if (tagId.HasValue)
{
    query = query.Where(t =>
        t.Tags.Any(tag => tag.Id == tagId.Value));
}

    // Sorting
    query = sort switch
    {
        "oldest" => query.OrderBy(t => t.CreatedAt),

        "dueDate" => query
            .OrderBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate),

        "priority" => query
            .OrderByDescending(t => t.Priority),

        _ => query.OrderByDescending(t => t.CreatedAt)
    };

    var todos = await query.ToListAsync();

// Statistics
var allTodos = await _context.Todos.ToListAsync();

ViewBag.TotalCount = allTodos.Count;

ViewBag.CompletedCount =
    allTodos.Count(t => t.IsCompleted);

ViewBag.PendingCount =
    allTodos.Count(t => !t.IsCompleted);

ViewBag.OverdueCount =
    allTodos.Count(t =>
        !t.IsCompleted &&
        t.DueDate.HasValue &&
        t.DueDate.Value.Date < DateTime.Today);

// Filter values
ViewBag.Search = search;
ViewBag.Status = status;
ViewBag.Priority = priority;
ViewBag.CategoryId = categoryId;
ViewBag.TagId = tagId;
ViewBag.Sort = sort;

// Categories
ViewBag.Categories = await _context.Categories
    .OrderBy(c => c.Name)
    .ToListAsync();

// Tags
ViewBag.Tags = await _context.Tags
    .OrderBy(t => t.Name)
    .ToListAsync();

return View(todos);}
   


// GET: /Todo/Create
public async Task<IActionResult> Create()
{
    ViewBag.Categories = await _context.Categories
        .OrderBy(c => c.Name)
        .ToListAsync();

    ViewBag.Tags = await _context.Tags
        .OrderBy(t => t.Name)
        .ToListAsync();

    return View();
}

// POST: /Todo/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Todo todo, int[] selectedTags)
{
    if (!ModelState.IsValid)
    {
        ViewBag.Categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Tags = await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();

        return View(todo);
    }

    if (selectedTags.Length > 0)
    {
        todo.Tags = await _context.Tags
            .Where(t => selectedTags.Contains(t.Id))
            .ToListAsync();
    }

    _context.Todos.Add(todo);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}




  // GET: /Todo/Edit/3
public async Task<IActionResult> Edit(int id)
{
    var todo = await _context.Todos
        .Include(t => t.Tags)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (todo == null)
    {
        return NotFound();
    }

    ViewBag.Categories = await _context.Categories
        .OrderBy(c => c.Name)
        .ToListAsync();

    ViewBag.Tags = await _context.Tags
        .OrderBy(t => t.Name)
        .ToListAsync();

    return View(todo);
}


// POST: /Todo/Edit/3
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(
    int id,
    Todo todo,
    int[] selectedTags)
{
    if (id != todo.Id)
    {
        return BadRequest();
    }

    if (!ModelState.IsValid)
    {
        ViewBag.Categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Tags = await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();

        return View(todo);
    }

    var existingTodo = await _context.Todos
        .Include(t => t.Tags)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (existingTodo == null)
    {
        return NotFound();
    }

    existingTodo.Title = todo.Title;
    existingTodo.Description = todo.Description;
    existingTodo.Priority = todo.Priority;
    existingTodo.DueDate = todo.DueDate;
    existingTodo.IsCompleted = todo.IsCompleted;
    existingTodo.CategoryId = todo.CategoryId;

    existingTodo.Tags.Clear();

    if (selectedTags.Length > 0)
    {
        var tags = await _context.Tags
            .Where(t => selectedTags.Contains(t.Id))
            .ToListAsync();

        foreach (var tag in tags)
        {
            existingTodo.Tags.Add(tag);
        }
    }

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}

    //post todo/delete/3
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null)
        {
            return NotFound();
        }
        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
}
//post todo/complete/3
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Complete(int id)
{
    var todo = await _context.Todos.FindAsync(id);

    if (todo == null)
    {
        return NotFound();
    }

    // Toggle completion status
    todo.IsCompleted = !todo.IsCompleted;

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}

}