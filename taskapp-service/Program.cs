using backend.Data;
using backend.Middleware;
using backend.Models;
using backend.Repositories;
using backend.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description =
            "Cookie-based session auth. Use **POST /api/Auth/login** (Try it out) first; the browser keeps the `taskapp.auth` cookie for other endpoints on this host."
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "taskapp.auth";
        // Angular (http://localhost:4200) calling API (https://localhost:7051) is cross-site;
        // Lax cookies are not sent on XHR/fetch — tasks would get 401 and bounce to login.
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/api/auth/login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["FrontendOrigin"] ?? "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "Task Management API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await SeedDataAsync(app.Services);
app.Run();

static async Task SeedDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    if (!await dbContext.Users.AnyAsync())
    {
        dbContext.Users.Add(new AppUser
        {
            Username = "admin",
            Password = "password123"
        });
    }

    if (!await dbContext.TaskItems.AnyAsync())
    {
        dbContext.TaskItems.AddRange(
            new TaskItem
            {
                Title = "Set up project",
                Description = "Backend and frontend scaffolding",
                Priority = "High",
                DueDate = DateTime.UtcNow.Date.AddDays(2),
                IsCompleted = false
            },
            new TaskItem
            {
                Title = "Implement CRUD",
                Description = "Add create/read/update/delete operations",
                Priority = "Medium",
                DueDate = DateTime.UtcNow.Date.AddDays(4),
                IsCompleted = false
            });
    }

    await dbContext.SaveChangesAsync();
}
