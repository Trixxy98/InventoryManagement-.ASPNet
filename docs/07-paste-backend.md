# 07 — Paste Backend (manual)

Anda sudah ada `backend/` (.NET 10, EF Core Sqlite). **Jangan** minta AI tulis terus ke folder `backend/`.

Port sebenar anda: `http://localhost:5150` (bukan 5088).

---

## Status sekarang


| Item                      | Ada?                        |
| ------------------------- | --------------------------- |
| `dotnet new webapi`       | Ya                          |
| EF Sqlite + Design        | Ya                          |
| WeatherForecast template  | Sudah dibuang               |
| Models / DbContext / seed | Ya — Fasa 1 siap            |
| Controllers inventory     | Belum — **Fasa 2 sekarang** |


---

# FASA 1 — skeleton (buat ini dulu, kemudian STOP)

Selepas Fasa 1: `dotnet run` berjaya + fail `backend/inventory.db` wujud.  
**Jangan paste controller lagi.**

## Langkah A — padam template (manual)

Padam 2 fail ini:

1. `backend/WeatherForecast.cs`
2. `backend/Controllers/WeatherForecastController.cs`



## Langkah B — cipta folder kosong

Dalam `backend/`:

```
Models/
Data/
DTOs/
Controllers/     ← sudah ada, biarkan
```

`DTOs/` untuk Fasa 2. Boleh cipta sekarang supaya struktur siap.

## Langkah C — cipta fail, paste satu per satu

Urutan penting: **models → DbContext → appsettings → Program.cs**

---



### Fail 1 — `backend/Models/Category.cs`

```csharp
namespace InventoryApi.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
```

---



### Fail 2 — `backend/Models/Product.cs`

```csharp
namespace InventoryApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
```

---



### Fail 3 — `backend/Models/StockTransaction.cs`

```csharp
namespace InventoryApi.Models;

public enum StockType
{
    In = 1,
    Out = 2
}

public class StockTransaction
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public StockType Type { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---



### Fail 4 — `backend/Models/StockStatuses.cs`

Status **tidak** disimpan dalam DB. Kira dari Quantity + MinimumStock.

```csharp
namespace InventoryApi.Models;

public static class StockStatuses
{
    public const string InStock = "InStock";
    public const string LowStock = "LowStock";
    public const string OutOfStock = "OutOfStock";

    public static string From(int quantity, int minimumStock)
    {
        if (quantity <= 0) return OutOfStock;
        if (quantity <= minimumStock) return LowStock;
        return InStock;
    }
}
```

---



### Fail 5 — `backend/Data/AppDbContext.cs`

```csharp
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Price).HasPrecision(18, 2);

            entity.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.Property(x => x.Note).HasMaxLength(255);

            entity.HasOne(x => x.Product)
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        var created = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Gadgets and accessories", CreatedAt = created },
            new Category { Id = 2, Name = "Stationery", Description = "Paper and pens", CreatedAt = created },
            new Category { Id = 3, Name = "Grocery", Description = "Daily essentials", CreatedAt = created }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "USB Cable", CategoryId = 1, Price = 9.90m, Quantity = 45, MinimumStock = 10, CreatedAt = created },
            new Product { Id = 2, Name = "Wireless Mouse", CategoryId = 1, Price = 29.00m, Quantity = 8, MinimumStock = 10, CreatedAt = created },
            new Product { Id = 3, Name = "A4 Paper Ream", CategoryId = 2, Price = 12.50m, Quantity = 0, MinimumStock = 5, CreatedAt = created },
            new Product { Id = 4, Name = "Blue Pen", CategoryId = 2, Price = 1.20m, Quantity = 200, MinimumStock = 50, CreatedAt = created },
            new Product { Id = 5, Name = "Instant Noodles", CategoryId = 3, Price = 2.50m, Quantity = 15, MinimumStock = 20, CreatedAt = created },
            new Product { Id = 6, Name = "Bottled Water", CategoryId = 3, Price = 1.00m, Quantity = 3, MinimumStock = 12, CreatedAt = created }
        );
    }
}
```

---



### Fail 6 — `backend/appsettings.json` (ganti seluruh fail)

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=inventory.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---



### Fail 7 — `backend/Program.cs` (ganti seluruh fail)

```csharp
using InventoryApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

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
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();
```

---



## Langkah D — run & sahkan Fasa 1

Dalam terminal:

```bash
cd backend
dotnet run
```

Patut nampak listening pada `http://localhost:5150`.

Checklist:

- [x] Compile tanpa error
- [x] Fail `backend/inventory.db` muncul
- [ ] OpenAPI JSON: buka `http://localhost:5150/openapi/v1.json` (controller inventory belum ada, itu normal)

Kalau anda tukar seed kemudian, **padam** `inventory.db` dan run semula. `EnsureCreated` tidak update DB yang sudah wujud.

**STOP di sini.** Jangan mula React. Jangan paste Fasa 2 sehingga `dotnet run` OK.

---



# FASA 2 — API lengkap (hanya lepas Fasa 1 hijau)

Paste DTOs dulu, kemudian controllers.

---



### Fail 8 — `backend/DTOs/CategoryDtos.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace InventoryApi.DTOs;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProductCount { get; set; }
}

public class CategoryRequest
{
    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}
```

---



### Fail 9 — `backend/DTOs/ProductDtos.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace InventoryApi.DTOs;

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public string StockStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateProductRequest
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumStock { get; set; }
}

public class UpdateProductRequest
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumStock { get; set; }
}
```

Tiada `Quantity` pada update — stok hanya melalui Stock In/Out.

---



### Fail 10 — `backend/DTOs/StockDtos.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace InventoryApi.DTOs;

public class StockMoveRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [MaxLength(255)]
    public string? Note { get; set; }
}

public class StockMoveResponse
{
    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int BalanceAfter { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockTransactionResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---



### Fail 11 — `backend/DTOs/DashboardDto.cs`

```csharp
namespace InventoryApi.DTOs;

public class DashboardResponse
{
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
}

public class LowStockItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public string StockStatus { get; set; } = string.Empty;
}
```

---



### Fail 12 — `backend/DTOs/ProductMapper.cs`

```csharp
using InventoryApi.Models;

namespace InventoryApi.DTOs;

public static class ProductMapper
{
    public static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            MinimumStock = product.MinimumStock,
            StockStatus = StockStatuses.From(product.Quantity, product.MinimumStock),
            CreatedAt = product.CreatedAt
        };
    }
}
```

---



### Fail 13 — `backend/Controllers/CategoriesController.cs`

```csharp
using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
    {
        var items = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var item = await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync();

        if (item is null)
        {
            return NotFound(new { message = "Category not found" });
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request)
    {
        var name = request.Name.Trim();
        var exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        if (exists)
        {
            return BadRequest(new { message = "Category name already exists" });
        }

        var entity = new Category
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();

        var response = new CategoryResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            ProductCount = 0
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request)
    {
        var entity = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null)
        {
            return NotFound(new { message = "Category not found" });
        }

        var name = request.Name.Trim();
        var exists = await _db.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());
        if (exists)
        {
            return BadRequest(new { message = "Category name already exists" });
        }

        entity.Name = name;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        await _db.SaveChangesAsync();

        return Ok(new CategoryResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            ProductCount = entity.Products.Count
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null)
        {
            return NotFound(new { message = "Category not found" });
        }

        if (entity.Products.Count > 0)
        {
            return Conflict(new { message = "Cannot delete category that has products" });
        }

        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
```

---



### Fail 14 — `backend/Controllers/ProductsController.cs`

```csharp
using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] string? stockStatus)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (categoryId is not null)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(stockStatus))
        {
            query = stockStatus.Trim().ToLower() switch
            {
                "in" => query.Where(p => p.Quantity > p.MinimumStock),
                "low" => query.Where(p => p.Quantity > 0 && p.Quantity <= p.MinimumStock),
                "out" => query.Where(p => p.Quantity == 0),
                _ => query
            };
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        return Ok(products.Select(ProductMapper.ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound(new { message = "Product not found" });
        }

        return Ok(ProductMapper.ToResponse(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var name = request.Name.Trim();

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new { message = "Category not found" });
        }

        var nameTaken = await _db.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower());
        if (nameTaken)
        {
            return BadRequest(new { message = "Product name already exists" });
        }

        var entity = new Product
        {
            Name = name,
            CategoryId = request.CategoryId,
            Price = request.Price,
            Quantity = request.Quantity,
            MinimumStock = request.MinimumStock,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(p => p.Category).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ProductMapper.ToResponse(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
    {
        var entity = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null)
        {
            return NotFound(new { message = "Product not found" });
        }

        var name = request.Name.Trim();

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new { message = "Category not found" });
        }

        var nameTaken = await _db.Products.AnyAsync(p => p.Id != id && p.Name.ToLower() == name.ToLower());
        if (nameTaken)
        {
            return BadRequest(new { message = "Product name already exists" });
        }

        entity.Name = name;
        entity.CategoryId = request.CategoryId;
        entity.Price = request.Price;
        entity.MinimumStock = request.MinimumStock;
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(p => p.Category).LoadAsync();
        return Ok(ProductMapper.ToResponse(entity));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity is null)
        {
            return NotFound(new { message = "Product not found" });
        }

        _db.Products.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
```

---



### Fail 15 — `backend/Controllers/StockController.cs`

```csharp
using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly AppDbContext _db;

    public StockController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("in")]
    public async Task<ActionResult<StockMoveResponse>> StockIn(StockMoveRequest request)
    {
        return await Move(request, StockType.In);
    }

    [HttpPost("out")]
    public async Task<ActionResult<StockMoveResponse>> StockOut(StockMoveRequest request)
    {
        return await Move(request, StockType.Out);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransactionResponse>>> History([FromQuery] int? productId)
    {
        var query = _db.StockTransactions.Include(t => t.Product).AsQueryable();

        if (productId is not null)
        {
            query = query.Where(t => t.ProductId == productId);
        }

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new StockTransactionResponse
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product.Name,
                Type = t.Type.ToString(),
                Quantity = t.Quantity,
                Note = t.Note,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    private async Task<ActionResult<StockMoveResponse>> Move(StockMoveRequest request, StockType type)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product is null)
        {
            return NotFound(new { message = "Product not found" });
        }

        if (type == StockType.Out && request.Quantity > product.Quantity)
        {
            return BadRequest(new { message = $"Insufficient stock. Available: {product.Quantity}" });
        }

        product.Quantity = type == StockType.In
            ? product.Quantity + request.Quantity
            : product.Quantity - request.Quantity;

        var transaction = new StockTransaction
        {
            ProductId = product.Id,
            Type = type,
            Quantity = request.Quantity,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.StockTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        return Ok(new StockMoveResponse
        {
            TransactionId = transaction.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            Type = type.ToString(),
            Quantity = transaction.Quantity,
            BalanceAfter = product.Quantity,
            Note = transaction.Note,
            CreatedAt = transaction.CreatedAt
        });
    }
}
```

---



### Fail 16 — `backend/Controllers/DashboardController.cs`

```csharp
using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get()
    {
        var totalProducts = await _db.Products.CountAsync();
        var totalStock = await _db.Products.SumAsync(p => (int?)p.Quantity) ?? 0;
        var lowStock = await _db.Products.CountAsync(p => p.Quantity > 0 && p.Quantity <= p.MinimumStock);
        var outOfStock = await _db.Products.CountAsync(p => p.Quantity == 0);

        return Ok(new DashboardResponse
        {
            TotalProducts = totalProducts,
            TotalStock = totalStock,
            LowStock = lowStock,
            OutOfStock = outOfStock
        });
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockItemResponse>>> LowStock()
    {
        var items = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Quantity <= p.MinimumStock)
            .ToListAsync();

        var response = items
            .OrderBy(p => p.Quantity == 0 ? 0 : 1)
            .ThenBy(p => p.Name)
            .Select(p => new LowStockItemResponse
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category.Name,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                StockStatus = StockStatuses.From(p.Quantity, p.MinimumStock)
            });

        return Ok(response);
    }
}
```

---



### Fail 17 — `backend/InventoryApi.http` (ganti, untuk test)

```http
@host = http://localhost:5150

### Dashboard
GET {{host}}/api/dashboard

### Products
GET {{host}}/api/products

### Search
GET {{host}}/api/products?search=mouse

### Low stock
GET {{host}}/api/products?stockStatus=low

### Stock in
POST {{host}}/api/stock/in
Content-Type: application/json

{
  "productId": 2,
  "quantity": 20,
  "note": "Supplier delivery"
}

### Stock out — patut 400 (Wireless Mouse baki 8)
POST {{host}}/api/stock/out
Content-Type: application/json

{
  "productId": 2,
  "quantity": 999,
  "note": "Should fail"
}
```

---



## Test Fasa 2 (Swagger UI tiada by default di .NET 10)

Guna `InventoryApi.http` (Send Request) **atau** browser:


| Check | URL / action                          | Jangkaan                                                              |
| ----- | ------------------------------------- | --------------------------------------------------------------------- |
| 1     | GET `/api/dashboard`                  | `totalProducts: 6`, `totalStock: 271`, `lowStock: 3`, `outOfStock: 1` |
| 2     | GET `/api/products`                   | 6 produk                                                              |
| 3     | GET `/api/products?search=mouse`      | Wireless Mouse                                                        |
| 4     | POST `/api/stock/out` qty 999         | 400 Insufficient stock                                                |
| 5     | POST `/api/stock/in` qty 20 pada id 2 | balanceAfter 28                                                       |
| 6     | DELETE `/api/categories/1`            | 409 (ada produk)                                                      |


Formula seed: 45+8+0+200+15+3 = **271**.