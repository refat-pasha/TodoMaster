using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;
using TodoMaster.Models;

namespace TodoMaster.Controllers;

public class TagController : Controller
{
    private readonly AppDbContext _context;

    public TagController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Tag
    public async Task<IActionResult> Index()
    {
        var tags = await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();

        return View(tags);
    }

    // GET: /Tag/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Tag/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Tag tag)
{
    if (await _context.Tags
        .AnyAsync(t => t.Name.ToLower() == tag.Name.ToLower()))
    {
        ModelState.AddModelError(
            "Name",
            "A tag with this name already exists.");
    }

    if (!ModelState.IsValid)
    {
        return View(tag);
    }

    _context.Tags.Add(tag);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}

    // GET: /Tag/Edit/3
    public async Task<IActionResult> Edit(int id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound();
        }

        return View(tag);
    }

    // POST: /Tag/Edit/3
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Tag tag)
{
    if (id != tag.Id)
    {
        return BadRequest();
    }

    if (await _context.Tags
        .AnyAsync(t =>
            t.Id != tag.Id &&
            t.Name.ToLower() == tag.Name.ToLower()))
    {
        ModelState.AddModelError(
            "Name",
            "A tag with this name already exists.");
    }

    if (!ModelState.IsValid)
    {
        return View(tag);
    }

    _context.Tags.Update(tag);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}

   // POST: /Tag/Delete/3
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var tag = await _context.Tags
        .Include(t => t.Todos)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (tag == null)
    {
        return NotFound();
    }

    tag.Todos.Clear();

    _context.Tags.Remove(tag);

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
}
}