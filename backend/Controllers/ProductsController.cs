using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase {
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] string? stockStatus)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (categoryId is not null) {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(stockStatus)) {
            query = stockStatus.Trim().ToLower() switch {
                "in" => query.Where(p => p.Quantity > p.MinimumStock),
                "low" => query.Where(p => p.Quantity > 0 && p.Quantity <= p.MinimumStock),
                "out" => query.Where(p => p.Quantity === 0),
                _ => query
            };
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        return Ok(products.Select(ProductMapper.ToResponse));
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

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) {
            return BadRequest(new {message = "Category not found"});
        }

        var nameTaken = await _db.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower());
        if (nameTaken) {
            return BadRequest(new {message = "Product name already exists"});
        }

        var entity = new Product {
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
        return CreatedAtAction(nameof(GetById), new {id = entity.Id}, ProductMapper.ToResponse(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request) {
        var entity = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null) {
            return NotFound(new {message = "Product not found"});
        }

        var name = request.Name.Trim();

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) {
            return BadRequest(new {message = "Category not found"});
        }

        var nameTaken = await _db.Products.AnyAsync(p => p.Id != id && p.Name.ToLower() == name.ToLower());
        if (nameTaken) {
            return BadRequest(new {message = "Product name already exists"});
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