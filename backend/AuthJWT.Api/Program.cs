using System.Text;
using AuthJWT.Api.Data;
using AuthJWT.Api.Endpoints;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/nexusdesk-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=authjwt.db"));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IWorkTaskService, WorkTaskService>();

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

// OpenAPI
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

    var admin = new User
    {
        Name = "Administrador",
        Email = "admin@nexusdesk.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
        Role = UserRoles.Administrator
    };
    db.Users.Add(admin);

    var employee = new User
    {
        Name = "Funcionário Teste",
        Email = "employee@nexusdesk.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"),
        Role = UserRoles.Employee
    };
    db.Users.Add(employee);

    await db.SaveChangesAsync();
}

app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("NexusDesk API")
       .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
});

app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCustomerEndpoints();
app.MapWorkTaskEndpoints();
app.MapTodoEndpoints();

app.Run();
