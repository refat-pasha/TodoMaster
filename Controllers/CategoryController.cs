using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;
using TodoMaster.Models;

namespace TodoMaster.Controllers;

public class CategoryController : Controller
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Category
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    // GET: /Category/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Category/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Category category)
{
    if (await _context.Categories
        .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower()))
    {
        ModelState.AddModelError(
            "Name",
            "A category with this name already exists.");
    }

    if (!ModelState.IsValid)
    {
        return View(category);
    }

    _context.Categories.Add(category);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}

    // GET: /Category/Edit/3
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }
// POST: /Category/Edit/3
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Category category)
{
    if (id != category.Id)
    {
        return BadRequest();
    }

    if (await _context.Categories
        .AnyAsync(c =>
            c.Id != category.Id &&
            c.Name.ToLower() == category.Name.ToLower()))
    {
        ModelState.AddModelError(
            "Name",
            "A category with this name already exists.");
    }

    if (!ModelState.IsValid)
    {
        return View(category);
    }

    _context.Categories.Update(category);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}

    // POST: /Category/Delete/3
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var category = await _context.Categories.FindAsync(id);

    if (category == null)
    {
        return NotFound();
    }

    var todos = await _context.Todos
        .Where(t => t.CategoryId == id)
        .ToListAsync();

    foreach (var todo in todos)
    {
        todo.CategoryId = null;
    }

    _context.Categories.Remove(category);

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}
}