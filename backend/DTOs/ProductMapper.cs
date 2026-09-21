using InventoryApi.Models;

namespace InventoryApi.DTOs;

public static class ProductMapper {
    public static ProductResponse ToResponse(Product product) {
        return new ProductResponse {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            MinimumStock = product.MinimumStock,
            StockStatus = StockStatuses.From(product.Quantity,
            product.MinimumStock),
            CreatedAt = product.CreatedAt
        };
    }
}