using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[Authorize]
[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase {
    private readonly AppDbContext _db;

    public StockController(AppDbContext db) {
        _db = db;
    }

    [HttpPost("in")]
    public async Task<ActionResult<StockMoveResponse>> StockIn(StockMoveRequest request) {
        return await Move(request, StockType.In);
    }

    [HttpPost("out")]
    public async Task<ActionResult<StockMoveResponse>> StockOut(StockMoveRequest request) {
        return await Move(request, StockType.Out);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransactionResponse>>> History([FromQuery] int? productId) {
        var query = _db.StockTransactions.Include(t => t.Product).AsQueryable();

        if (productId is not null) {
            query = query.Where(t => t.ProductId == productId);
        }

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new StockTransactionResponse {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product.Name,
                Type = t.Type.ToString(),
                Quantity = t.Quantity,
                Note = t.Note,
                CreatedAt = t.CreatedAt,
            })
            .ToListAsync();

        return Ok(items);
    }

    private async Task<ActionResult<StockMoveResponse>> Move(StockMoveRequest request, StockType type) {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product is null) {
            return NotFound(new {message = "Product not found"});
        }

        if (type == StockType.Out && request.Quantity > product.Quantity) {
            return BadRequest(new {message = $"Insufficient stock. Available: {product.Quantity}"});
        }

        product.Quantity = type == StockType.In
            ? product.Quantity + request.Quantity
            : product.Quantity - request.Quantity;

        var transaction = new StockTransaction {
            ProductId = product.Id,
            Type = type,
            Quantity = request.Quantity,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.StockTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        return Ok(new StockMoveResponse {
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