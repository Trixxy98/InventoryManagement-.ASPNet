using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase {
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get() {
        var totalProducts = await _db.Products.CountAsync();
        var totalStock = await _db.Products.SumAsync(p => (int?)p.Quantity) ?? 0;
        var lowStock = await _db.Products.CountAsync(p => p.Quantity > 0 && p.Quantity <= p.MinimumStock);
        var outOfStock = await _db.Products.CountAsync(p => p.Quantity == 0);

        return Ok(new DashboardResponse {
            TotalProducts = totalProducts,
            TotalStock = totalStock,
            LowStock = lowStock,
            OutOfStock = outOfStock,
        });
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockItemResponse>>> LowStock() {
        var items = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Quantity <= p.MinimumStock)
            .ToListAsync();

        var response = items 
            .OrderBy(p => p.Quantity == 0 ? 0 : 1)
            .ThenBy(p => p.Name)
            .Select(p => new LowStockItemResponse {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category.Name,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                StockStatus = StockStatuses.From(p.Quantity, p.MinimumStock)
            });

        return Ok(response);
    }
}