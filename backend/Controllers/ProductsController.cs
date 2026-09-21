using System.Text;
using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase {
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] string? stockStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = ApplyFilters(_db.Products.Include(p => p.Category), search, categoryId, stockStatus);
        var total = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<ProductResponse>
        {
            Items = products.Select(ProductMapper.ToResponse),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] string? stockStatus)
    {
        var products = await ApplyFilters(_db.Products.Include(p => p.Category), search, categoryId, stockStatus)
            .OrderBy(p => p.Name)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Sku,Name,Category,Price,Quantity,MinimumStock,Status");
        foreach (var p in products)
        {
            var status = StockStatuses.From(p.Quantity, p.MinimumStock);
            csv.AppendLine($"{Csv(p.Sku)},{Csv(p.Name)},{Csv(p.Category.Name)},{p.Price},{p.Quantity},{p.MinimumStock},{status}");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "products.csv");
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id) {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) {
            return NotFound(new {message = "Product not found"});
        }

        return Ok(ProductMapper.ToResponse(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request) {
        var name = request.Name.Trim();
        var sku = request.Sku.Trim().ToUpperInvariant();

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) {
            return BadRequest(new {message = "Category not found"});
        }

        if (await _db.Products.IgnoreQueryFilters().AnyAsync(p => p.Name.ToLower() == name.ToLower())) {
            return BadRequest(new {message = "Product name already exists"});
        }

        if (await _db.Products.IgnoreQueryFilters().AnyAsync(p => p.Sku.ToLower() == sku.ToLower())) {
            return BadRequest(new {message = "SKU already exists"});
        }

        var entity = new Product {
            Name = name,
            Sku = sku,
            CategoryId = request.CategoryId,
            Price = request.Price,
            Quantity = request.Quantity,
            MinimumStock = request.MinimumStock,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(p => p.Category).LoadAsync();
        return CreatedAtAction(nameof(GetById), new {id = entity.Id}, ProductMapper.ToResponse(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request) {
        var entity = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null) {
            return NotFound(new {message = "Product not found"});
        }

        var name = request.Name.Trim();
        var sku = request.Sku.Trim().ToUpperInvariant();

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) {
            return BadRequest(new {message = "Category not found"});
        }

        if (await _db.Products.IgnoreQueryFilters().AnyAsync(p => p.Id != id && p.Name.ToLower() == name.ToLower())) {
            return BadRequest(new {message = "Product name already exists"});
        }

        if (await _db.Products.IgnoreQueryFilters().AnyAsync(p => p.Id != id && p.Sku.ToLower() == sku.ToLower())) {
            return BadRequest(new {message = "SKU already exists"});
        }

        entity.Name = name;
        entity.Sku = sku;
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
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null)
        {
            return NotFound(new { message = "Product not found" });
        }

        entity.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static IQueryable<Product> ApplyFilters(
        IQueryable<Product> query,
        string? search,
        int? categoryId,
        string? stockStatus)
    {
        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Sku.ToLower().Contains(term));
        }

        if (categoryId is not null) {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(stockStatus)) {
            query = stockStatus.Trim().ToLower() switch {
                "in" => query.Where(p => p.Quantity > p.MinimumStock),
                "low" => query.Where(p => p.Quantity > 0 && p.Quantity <= p.MinimumStock),
                "out" => query.Where(p => p.Quantity == 0),
                _ => query
            };
        }

        return query;
    }

    private static string Csv(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
