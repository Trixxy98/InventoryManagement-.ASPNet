using System.Text;
using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedRuntimeData(db);
}

app.UseCors("Frontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void SeedRuntimeData(AppDbContext db)
{
    if (!db.Users.Any())
    {
        var hasher = new PasswordHasher<User>();
        var admin = new User { Username = "admin" };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
        db.Users.Add(admin);
    }

    if (!db.StockTransactions.Any())
    {
        var today = DateTime.UtcNow.Date;
        db.StockTransactions.AddRange(
            new StockTransaction { ProductId = 1, Type = StockType.In, Quantity = 12, Note = "Seed", CreatedAt = today.AddDays(-6) },
            new StockTransaction { ProductId = 2, Type = StockType.Out, Quantity = 3, Note = "Seed", CreatedAt = today.AddDays(-5) },
            new StockTransaction { ProductId = 4, Type = StockType.In, Quantity = 40, Note = "Seed", CreatedAt = today.AddDays(-4) },
            new StockTransaction { ProductId = 5, Type = StockType.Out, Quantity = 8, Note = "Seed", CreatedAt = today.AddDays(-3) },
            new StockTransaction { ProductId = 1, Type = StockType.Out, Quantity = 5, Note = "Seed", CreatedAt = today.AddDays(-2) },
            new StockTransaction { ProductId = 6, Type = StockType.In, Quantity = 24, Note = "Seed", CreatedAt = today.AddDays(-1) },
            new StockTransaction { ProductId = 2, Type = StockType.In, Quantity = 10, Note = "Seed", CreatedAt = today }
        );
    }

    db.SaveChanges();
}
