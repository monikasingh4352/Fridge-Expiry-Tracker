using System.Security.Claims;
using FridgeApi.Data;
using FridgeApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database: SQLite file, path comes from appsettings.json (ConnectionStrings:Fridge).
var connectionString = builder.Configuration.GetConnectionString("Fridge")
    ?? "Data Source=fridge.db";
builder.Services.AddDbContext<FridgeDbContext>(options => options.UseSqlite(connectionString));

// Login: after a successful login the browser gets a secure cookie that it sends with every request.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "fridge.auth";
        options.Cookie.HttpOnly = true;                 // JavaScript cannot read the cookie
        options.Cookie.SameSite = SameSiteMode.Strict;  // not sent from other websites
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // This is an API: answer 401/403 instead of redirecting to a login page.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Create the database and tables on first run.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<FridgeDbContext>().Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);

static Task SignInAsync(HttpContext http, User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    return http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
}

var auth = app.MapGroup("/api/auth");

// POST /api/auth/register: create an account and log in.
auth.MapPost("/register", async (RegisterRequest request, FridgeDbContext db, HttpContext http) =>
{
    var errors = request.Validate();
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var email = request.NormalizedEmail;
    if (await db.Users.AnyAsync(u => u.Email == email))
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { ["email"] = ["An account with this email already exists."] },
            statusCode: StatusCodes.Status409Conflict);

    var user = new User
    {
        Name = request.Name!.Trim(),
        Phone = request.Phone!.Trim(),
        Email = email,
        PasswordHash = PasswordHashing.Hash(request.Password!),
        CreatedOn = DateTime.UtcNow
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

    await SignInAsync(http, user);
    return Results.Ok(new UserResponse(user.Name, user.Email));
});

// POST /api/auth/login: check email and password, then log in.
auth.MapPost("/login", async (LoginRequest request, FridgeDbContext db, HttpContext http) =>
{
    var email = request.NormalizedEmail;
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    // Same message whether the email or the password is wrong, so nobody can find out which emails exist.
    if (user is null || string.IsNullOrEmpty(request.Password)
        || !PasswordHashing.Verify(request.Password, user.PasswordHash))
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { ["login"] = ["Email or password is wrong."] },
            statusCode: StatusCodes.Status401Unauthorized);

    await SignInAsync(http, user);
    return Results.Ok(new UserResponse(user.Name, user.Email));
});

// POST /api/auth/logout: remove the login cookie.
auth.MapPost("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.NoContent();
});

// GET /api/auth/me: who is logged in (401 if nobody).
auth.MapGet("/me", (ClaimsPrincipal user) =>
        new UserResponse(user.FindFirstValue(ClaimTypes.Name) ?? "", user.FindFirstValue(ClaimTypes.Email)!))
    .RequireAuthorization();

// GET /api/categories: the list of categories the user can pick from.
app.MapGet("/api/categories", () => Categories.All).RequireAuthorization();

// Everything under /api/items needs a logged-in user.
var items = app.MapGroup("/api/items").RequireAuthorization();

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
        Category = Categories.Match(request.Category)!,
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
