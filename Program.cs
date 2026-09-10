using Microsoft.EntityFrameworkCore;
using TodoMaster.Data;

var builder = WebApplication.CreateBuilder(args);
//Add MVC
builder.Services.AddControllersWithViews();

//add EF core
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Datasource=todomaster.db"));

var app =  builder.Build();

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
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
