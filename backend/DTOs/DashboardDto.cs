namespace InventoryApi.DTOs;

public class DashboardResponse {
    public int TotalProducts {get; set;}
    public int TotalStock {get; set;}
    public int LowStock {get; set;}
    public int OutOfStock {get; set;}
}

public class LowStockItemResponse {
    public int Id {get; set;}
    public string Name {get; set;} =string.Empty;
    public string CategoryName {get; set;} = string.Empty;
    public int Quantity {get; set;}
    public int MinimumStock {get; set;}
    public string StockStatus {get; set;} = string.Empty;
}