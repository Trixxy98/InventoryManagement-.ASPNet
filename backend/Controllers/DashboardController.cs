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
                CategoryName = p.Category.Name,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                StockStatus = StockStatuses.From(p.Quantity, p.MinimumStock)
            });

        return Ok(response);
    }

    [HttpGet("movements")]
    public async Task<ActionResult<IEnumerable<StockMovementPoint>>> Movements([FromQuery] int days = 7)
    {
        days = Math.Clamp(days, 1, 31);
        var start = DateTime.UtcNow.Date.AddDays(1 - days);

        var rows = await _db.StockTransactions
            .Where(t => t.CreatedAt >= start)
            .ToListAsync();

        var lookup = rows
            .GroupBy(t => t.CreatedAt.Date)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    StockIn = g.Where(t => t.Type == StockType.In).Sum(t => t.Quantity),
                    StockOut = g.Where(t => t.Type == StockType.Out).Sum(t => t.Quantity)
                });
        var points = new List<StockMovementPoint>();
        for (var i = 0; i < days; i++)
        {
            var day = start.AddDays(i);
            lookup.TryGetValue(day, out var row);
            points.Add(new StockMovementPoint
            {
                Date = day.ToString("yyyy-MM-dd"),
                StockIn = row?.StockIn ?? 0,
                StockOut = row?.StockOut ?? 0
            });
        }

        return Ok(points);
    }
}