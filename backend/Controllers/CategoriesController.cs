using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase {
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db) {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll() {
        var items = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse {
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
    public async Task<ActionResult<CategoryResponse>> GetById(int id) {
        var item = await _db.Categories
            .Where(c => c.Name)
            .Select(c => new categoryResponse {
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
    public async Task<ActionResult<CategoryResponse>> GetById(int id) {
        var item = await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync();

        if (item is null) {
            return NotFound(new {message = "Category not found"});
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request) {
        var name = request.Name.Trim();
        var exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        if (exists) {
            return BadRequest(new {message = "Category name already exists"});
        }

        var entity = new Category {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();

        var response = new CategoryResponse {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            ProductCount = 0
        };

        return CreatedAtAction(nameof(GetById), new {id = entity.Id}, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request) {
        var entity = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) {
            return NotFound(new {message = "Category not found"});
        }

        var name = request.Name.Trim();
        var exists = await _db.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());
        if (exists) {
            return BadRequest(new {message = "Category name already exists"});
        }

        entity.Name = name;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        await _db.SaveChangesAsync();

        return Ok(new CategoryResponse {
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
