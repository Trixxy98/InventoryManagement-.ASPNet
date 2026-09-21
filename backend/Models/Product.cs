namespace InventoryApi.Models;

public class Product {
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Sku {get; set;} = string.Empty;
    public int CategoryId {get; set;}
    public Category Category {get; set;} = null!;
    public decimal Price {get; set;}
    public int Quantity {get; set;}
    public int MinimumStock {get; set;}
    public DateTime CreatedAt {get; set;}
    public bool IsDeleted {get; set;}

    public ICollection<StockTransaction> StockTransactions {get; set;} = new List<StockTransaction>
();
}