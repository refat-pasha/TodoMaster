using Microsoft.EntityFrameworkCore;
using TodoMaster.Models;

namespace TodoMaster.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }

    public DbSet<Category> Categories { get; set; }
}