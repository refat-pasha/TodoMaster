# TodoMaster

A beginner-friendly **Todo Management Web Application** built with **ASP.NET Core MVC, Razor Views, Entity Framework Core, and SQLite**.

This project is being developed as a practical learning project to understand how a complete ASP.NET Core MVC application works—from the browser request all the way to the database.

---

## 🚀 Project Status

**Current stage:** ASP.NET Core MVC + Entity Framework Core + SQLite

### Completed

* [x] ASP.NET Core MVC project setup
* [x] Todo model
* [x] Data validation
* [x] Entity Framework Core integration
* [x] SQLite database
* [x] EF Core migrations
* [x] Todo listing
* [x] Create Todo
* [x] Edit Todo
* [x] Delete Todo
* [x] Complete / Undo Todo
* [x] Todo priority
* [x] Due date
* [x] Server-side validation
* [x] Razor Views
* [x] Dependency Injection
* [x] Async database operations

### Planned

* [ ] Search
* [ ] Filtering
* [ ] Sorting
* [ ] Categories / Tags
* [ ] Better UI/UX
* [ ] Service layer
* [ ] ViewModels
* [ ] REST API
* [ ] Angular frontend
* [ ] Authentication & Authorization
* [ ] User-specific Todos
* [ ] Testing
* [ ] Deployment

---

# 📖 About The Project

TodoMaster is a simple Todo management application created to learn the fundamentals of **ASP.NET Core MVC**.

Instead of only following tutorials, this project focuses on understanding how the different parts of an ASP.NET Core application communicate with each other.

The current application allows users to:

1. View their Todos
2. Create new Todos
3. Edit existing Todos
4. Delete Todos
5. Mark Todos as completed
6. Undo completed Todos
7. Set a priority
8. Set a due date
9. Validate Todo input

---

# 🛠️ Technologies Used

| Technology               | Purpose                       |
| ------------------------ | ----------------------------- |
| C#                       | Programming language          |
| .NET 10                  | Application framework/runtime |
| ASP.NET Core MVC         | Web application framework     |
| Razor Views              | Server-side HTML rendering    |
| Entity Framework Core 10 | ORM / database access         |
| SQLite                   | Database                      |
| Bootstrap                | UI styling                    |
| Data Annotations         | Model validation              |
| LINQ                     | Querying data                 |
| Dependency Injection     | Application architecture      |
| EF Core Migrations       | Database schema management    |

---

# 📂 Project Structure

```text
TodoMaster/
│
├── Controllers/
│   ├── HomeController.cs
│   └── TodoController.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── Migrations/
│   ├── 20260910153828_InitialCreate.cs
│   ├── 20260910153828_InitialCreate.Designer.cs
│   └── AppDbContextModelSnapshot.cs
│
├── Models/
│   ├── Todo.cs
│   └── ErrorViewModel.cs
│
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   │
│   ├── Todo/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   │
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   │
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
│
├── Program.cs
├── TodoMaster.csproj
├── todomasters.db
└── README.md
```

---

# 🧱 Architecture

TodoMaster currently follows the **MVC architecture**.

```text
                    Browser
                       │
                       │ HTTP Request
                       ▼
                ┌──────────────┐
                │  Controller  │
                │              │
                │ TodoController
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │   DbContext  │
                │              │
                │ AppDbContext │
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │ Entity       │
                │ Framework    │
                │ Core         │
                └──────┬───────┘
                       │
                       ▼
                ┌──────────────┐
                │   SQLite     │
                │              │
                │ Todos table  │
                └──────────────┘
                       │
                       ▼
                  Data returned
                       │
                       ▼
                ┌──────────────┐
                │ Razor View   │
                └──────┬───────┘
                       │
                       ▼
                    Browser
```

---

# 📦 Todo Model

The main model is located at:

```text
Models/Todo.cs
```

The current Todo model contains:

```csharp
public class Todo
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;
}
```

Priority is represented by an enum:

```csharp
public enum Priority
{
    Low,
    Medium,
    High
}
```

---

# 🗄️ Database

TodoMaster currently uses **SQLite**.

The database connection is configured in:

```text
Program.cs
```

with:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=todomasters.db"));
```

The SQLite database file is:

```text
todomasters.db
```

---

# 🧩 Entity Framework Core

The database context is:

```text
Data/AppDbContext.cs
```

It contains:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }
}
```

`DbSet<Todo>` represents the `Todos` table in the database.

---

# 🔄 Database Migrations

Entity Framework Core migrations are used to create and update the database schema.

The first migration was created with:

```bash
dotnet ef migrations add InitialCreate
```

The database was then created/updated using:

```bash
dotnet ef database update
```

The resulting database contains a `Todos` table with:

```text
Todos
├── Id
├── Title
├── Description
├── IsCompleted
├── CreatedAt
├── DueDate
└── Priority
```

---

# 🎮 Todo Controller

The main controller is:

```text
Controllers/TodoController.cs
```

It currently provides the following actions:

| Action           | HTTP | Purpose                |
| ---------------- | ---- | ---------------------- |
| `Index()`        | GET  | Display all Todos      |
| `Create()`       | GET  | Display creation form  |
| `Create(Todo)`   | POST | Create a Todo          |
| `Edit(id)`       | GET  | Display edit form      |
| `Edit(id, Todo)` | POST | Update a Todo          |
| `Delete(id)`     | POST | Delete a Todo          |
| `Complete(id)`   | POST | Complete / undo a Todo |

---

# 📝 Creating a Todo

Navigate to:

```text
/Todo/Create
```

The user can provide:

* Title
* Description
* Priority
* Due Date

The submitted Todo is validated before it is stored.

The controller checks:

```csharp
if (!ModelState.IsValid)
{
    return View(todo);
}
```

If validation succeeds:

```csharp
_context.Todos.Add(todo);

await _context.SaveChangesAsync();
```

The Todo is then saved to SQLite.

---

# ✏️ Editing a Todo

Navigate to:

```text
/Todo/Edit/{id}
```

For example:

```text
/Todo/Edit/5
```

The existing Todo is retrieved from the database:

```csharp
var todo = await _context.Todos.FindAsync(id);
```

After editing, the updated Todo is saved:

```csharp
_context.Todos.Update(todo);

await _context.SaveChangesAsync();
```

---

# ✅ Completing a Todo

The application includes a Complete / Undo feature.

The controller toggles the value:

```csharp
todo.IsCompleted = !todo.IsCompleted;
```

Therefore:

```text
false → true
true  → false
```

The UI also visually changes completed Todos.

---

# 🗑️ Deleting a Todo

Deleting a Todo is performed through a POST request.

The controller finds the Todo:

```csharp
var todo = await _context.Todos.FindAsync(id);
```

Then removes it:

```csharp
_context.Todos.Remove(todo);

await _context.SaveChangesAsync();
```

The UI also asks for confirmation before deletion.

---

# ✔️ Validation

The Todo model currently uses Data Annotations.

For example:

```csharp
[Required]
[StringLength(100)]
public string Title { get; set; } = string.Empty;
```

This means:

* Title is required
* Maximum title length is 100 characters

Description has:

```csharp
[StringLength(500)]
```

which limits it to 500 characters.

Validation errors are displayed in the Razor views.

---

# 🖥️ Razor Views

TodoMaster currently contains three main Todo views:

### Todo List

```text
Views/Todo/Index.cshtml
```

Displays all Todos.

### Create

```text
Views/Todo/Create.cshtml
```

Provides the Todo creation form.

### Edit

```text
Views/Todo/Edit.cshtml
```

Provides the Todo editing form.

The views use ASP.NET Core Tag Helpers such as:

```cshtml
asp-action
asp-route-id
asp-for
asp-validation-for
asp-validation-summary
```

---

# 🔗 Important Routes

| URL            | Description         |
| -------------- | ------------------- |
| `/Todo`        | Todo list           |
| `/Todo/Index`  | Todo list           |
| `/Todo/Create` | Create Todo         |
| `/Todo/Edit/1` | Edit Todo with ID 1 |

The default MVC route is:

```text
{controller=Home}/{action=Index}/{id?}
```

---

# ⚙️ Getting Started

## Prerequisites

Install:

* .NET 10 SDK
* Git

Check your .NET installation:

```bash
dotnet --version
```

---

# 📥 Clone the Repository

```bash
git clone <YOUR-GITHUB-REPOSITORY-URL>
```

Enter the project:

```bash
cd TodoMaster
```

---

# 📦 Restore Dependencies

Run:

```bash
dotnet restore
```

---

# 🗄️ Create the Database

If the SQLite database is not included in the repository, create it from the migrations:

```bash
dotnet ef database update
```

If `dotnet ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

---

# ▶️ Run the Application

Start the application:

```bash
dotnet run
```

ASP.NET Core will display the local URLs in the terminal.

Open the application in your browser and navigate to:

```text
/Todo
```

---

# 🧪 Development Workflow

When changing the Todo model, database schema, or Entity Framework configuration:

### 1. Modify the model

For example:

```csharp
public string? Category { get; set; }
```

### 2. Create a migration

```bash
dotnet ef migrations add AddCategory
```

### 3. Apply the migration

```bash
dotnet ef database update
```

### 4. Run the application

```bash
dotnet run
```

---

# 🧠 What This Project Teaches

This project is designed to teach the fundamentals of ASP.NET Core MVC through practical development.

Important concepts currently covered:

### C#

* Classes
* Properties
* Enums
* Nullable types
* Async / Await
* Generics
* LINQ

### ASP.NET Core

* MVC architecture
* Controllers
* Actions
* Routing
* Dependency Injection
* Middleware
* Razor Views
* Tag Helpers
* Model Binding
* Model Validation

### Entity Framework Core

* DbContext
* DbSet
* LINQ queries
* CRUD operations
* Async database operations
* Migrations
* SQLite

---

# 🔍 Example Request Flow

When a user opens:

```text
/Todo
```

the request reaches:

```csharp
TodoController.Index()
```

The controller queries the database:

```csharp
var todos = await _context.Todos
    .OrderByDescending(t => t.CreatedAt)
    .ToListAsync();
```

The result is passed to:

```text
Views/Todo/Index.cshtml
```

The Razor view then generates HTML and sends it back to the browser.

In simplified form:

```text
GET /Todo
      ↓
TodoController.Index()
      ↓
AppDbContext
      ↓
SQLite
      ↓
List<Todo>
      ↓
Index.cshtml
      ↓
HTML
      ↓
Browser
```

---

# 🛡️ Security Practices Currently Used

The application currently uses:

* `[ValidateAntiForgeryToken]` on POST actions
* Model validation
* Entity Framework Core parameterized database operations
* Server-side validation
* POST requests for state-changing operations

Example:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    ...
}
```

---

# 🚧 Future Architecture

The long-term goal is to evolve TodoMaster into a more complete full-stack application.

The planned architecture is:

```text
                Angular Frontend
                       │
                       │ HTTP / JSON
                       ▼
              ASP.NET Core Web API
                       │
                       ▼
                Service Layer
                       │
                       ▼
              Entity Framework Core
                       │
                       ▼
                    SQLite
```

The current MVC application is therefore the **first stage** of the project.

Future stages will introduce:

```text
ASP.NET Core MVC
       ↓
EF Core + SQLite
       ↓
Web API
       ↓
Angular
       ↓
Authentication
       ↓
Production-ready application
```

---

# 📌 Learning Philosophy

TodoMaster is being built incrementally.

The goal is not simply to copy code, but to understand:

> **Why each piece exists and how the pieces communicate.**

Every major feature is introduced by understanding:

```text
Model
  ↓
Controller
  ↓
Database
  ↓
View
  ↓
Browser
```

---

# 📜 License

This project is currently intended as a personal learning project.

You can add a license here later, such as MIT, depending on how you want to distribute the project.

---

# 👨‍💻 Author

**Dev Master**

Built as a practical journey into:

* C#
* ASP.NET Core
* Entity Framework Core
* Web APIs
* Angular
* Full-stack web development

---

## ⭐ Future Goal

The final goal is to transform TodoMaster from a simple MVC Todo application into a modern full-stack application using:

**ASP.NET Core Web API + Entity Framework Core + Angular + TypeScript**

while keeping the project understandable enough to serve as a reference for learning ASP.NET Core from the ground up.
