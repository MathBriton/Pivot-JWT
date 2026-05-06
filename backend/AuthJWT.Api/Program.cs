using System.Text;
using AuthJWT.Api.Data;
using AuthJWT.Api.Endpoints;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=authjwt.db"));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITodoService, TodoService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS for frontend
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// OpenAPI (built-in .NET 10)
builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-migrate and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedAsync(db);
}

static async Task SeedAsync(AppDbContext db)
{
    if (db.Users.Any()) return;

    var user = new AuthJWT.Api.Models.User
    {
        Name = "Test User",
        Email = "test@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
        Role = "User"
    };
    db.Users.Add(user);

    await db.SaveChangesAsync();

    db.Todos.AddRange(
        new AuthJWT.Api.Models.TodoItem { Title = "Learn .NET 10 Minimal API", Description = "Study the new minimal API features", UserId = user.Id },
        new AuthJWT.Api.Models.TodoItem { Title = "Build a React app", Description = "Create a full stack project with TypeScript", UserId = user.Id },
        new AuthJWT.Api.Models.TodoItem { Title = "Configure JWT Auth", Description = "Set up authentication with JWT Bearer tokens", IsCompleted = true, CompletedAt = DateTime.UtcNow, UserId = user.Id }
    );
    await db.SaveChangesAsync();
}

app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("AuthJWT API")
       .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapTodoEndpoints();

app.Run();
