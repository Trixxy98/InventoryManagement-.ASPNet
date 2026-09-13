namespace InventoryApi.Models;

public static class StockStatuses {
    public const string InStock = "InStock";
    public const string LowStock = "LowStock";
    public const string OutOfStock = "OutOfStock";

    public static string From(int quantity, int minimumStock) {
        if (quantity <= 0) return OutOfStock;
        if (quantity <= minimumStock) return LowStock;
        return InStock;
    }

}