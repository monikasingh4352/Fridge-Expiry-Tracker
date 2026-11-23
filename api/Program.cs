using FridgeApi.Data;
using FridgeApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database: SQLite file, path comes from appsettings.json (ConnectionStrings:Fridge).
var connectionString = builder.Configuration.GetConnectionString("Fridge")
    ?? "Data Source=fridge.db";
builder.Services.AddDbContext<FridgeDbContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

// Create the database and table on first run.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<FridgeDbContext>().Database.EnsureCreated();
}

static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);

var items = app.MapGroup("/api/items");

// GET /api/items: all items, the ones that expire first at the top.
items.MapGet("/", async (FridgeDbContext db) =>
{
    var today = Today();
    var all = await db.FoodItems.AsNoTracking().ToListAsync();
    return all
        .OrderBy(i => i.ExpiryDate)
        .ThenBy(i => i.Name)
        .Select(i => FoodItemResponse.From(i, today));
});

// POST /api/items: add a new item.
items.MapPost("/", async (CreateFoodItemRequest request, FridgeDbContext db) =>
{
    var errors = request.Validate();
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var today = Today();
    var item = new FoodItem
    {
        Name = request.Name!.Trim(),
        Quantity = request.Quantity ?? 1,
        ExpiryDate = request.ExpiryDate!.Value,
        AddedOn = today
    };
    db.FoodItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/items/{item.Id}", FoodItemResponse.From(item, today));
});

// DELETE /api/items/{id}: remove an item (used up or thrown away).
items.MapDelete("/{id:int}", async (int id, FridgeDbContext db) =>
{
    var item = await db.FoodItems.FindAsync(id);
    if (item is null)
        return Results.NotFound();

    db.FoodItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
