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
    //get /todo
   public async Task<IActionResult> Index(
    string? search,
    string? status,
    Priority? priority,
    string? sort)
{
    var query = _context.Todos.AsQueryable();

    // Search by title or description
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(t =>
            t.Title.Contains(search) ||
            (t.Description != null && t.Description.Contains(search)));
    }

    // Filter by completion status
    if (status == "completed")
    {
        query = query.Where(t => t.IsCompleted);
    }
    else if (status == "pending")
    {
        query = query.Where(t => !t.IsCompleted);
    }

    // Filter by priority
    if (priority.HasValue)
    {
        query = query.Where(t => t.Priority == priority.Value);
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

    ViewBag.Search = search;
    ViewBag.Status = status;
    ViewBag.Priority = priority;
    ViewBag.Sort = sort;

    return View(todos);
}

    // get todo/create
    public IActionResult Create()
    {
        return View();
    }

    //post todo/create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Todo todo)
    {
        if (!ModelState.IsValid)
        {
            return View(todo);
        }
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    //get tpdp/edit/3
    public async Task<IActionResult> Edit(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if(todo == null)
        {
            return NotFound();
        }
        return View(todo);
    }

    //post todo/edit/3
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Todo todo)
    {
        if (id != todo.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(todo);
        }

        _context.Todos.Update(todo);
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
        todo.IsCompleted = true;
        _context.Todos.Update(todo);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}