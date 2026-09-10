using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;
using TodoMaster.Models;

namespace TodoMaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TodoApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TodoApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
        {
            var todos = await _context.Todos
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(todos);
        }

        // GET: api/TodoApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return Ok(todo);
        }

        // POST: api/TodoApi
        [HttpPost]
        public async Task<ActionResult<Todo>> CreateTodo(Todo todo)
        {
            todo.CreatedAt = DateTime.UtcNow;

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTodo),
                new { id = todo.Id },
                todo);
        }

        // PUT: api/TodoApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(
            int id,
            Todo todo)
        {
            if (id != todo.Id)
            {
                return BadRequest();
            }

            var existingTodo = await _context.Todos.FindAsync(id);

            if (existingTodo == null)
            {
                return NotFound();
            }

            existingTodo.Title = todo.Title;
            existingTodo.Description = todo.Description;
            existingTodo.IsCompleted = todo.IsCompleted;
            existingTodo.DueDate = todo.DueDate;
            existingTodo.Priority = todo.Priority;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/TodoApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}