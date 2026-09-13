namespace InventoryApi.Models;

public enum StockType {
    In = 1,
    Out = 2
}

public class StockTransaction {
    public int Id {get; set;}
    public int ProductId {get; set;}
    public Product Product {get; set;} = null!;
    public StockType Type {get; set;}
    public int Quantity {get; set;}
    public string? Note {get; set;}
    public DateTime CreatedAt {get; set;}
}
