using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;
using TodoMaster.Services;

var builder = WebApplication.CreateBuilder(args);
//Add MVC
builder.Services.AddControllersWithViews();

//add EF core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Todo Service
builder.Services.AddScoped<ITodoService, TodoService>();

var app = builder.Build();

// middleware config

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=todo}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
