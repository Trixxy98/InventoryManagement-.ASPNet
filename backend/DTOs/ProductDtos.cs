using System.ComponentModel.DataAnnotations;

namespace InventoryApi.DTOs;

public class ProductResponse {
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public int CategoryId {get; set;}
    public string CategoryName {get; set;} = string.Empty;
    public decimal Price {get; set;}
    public int Quantity {get; set;}
    public int MinimumStock {get; set;}
    public string StockStatus {get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;}
}

public class CreateProductRequest {
    [Required]
    [MaxLength(120)]
    public string Name {get; set;} = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId {get; set;}

    [Range(0, double.MaxValue)]
    public decimal Price {get; set;}

    [Range(0, int.MaxValue)]
    public int Quantity {get; set;}

    [Range(0, int.MaxValue)]
    public int MinimumStock {get; set;}
}

public class UpdateProductRequest {
    [Required]
    [MaxLength(120)]
    public string Name {get; set;} = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId {get; set;}
    
    [Range(0, double.MaxValue)]
    public decimal Price {get; set;}

    [Range(0, int.MaxValue)]
    public int MinimumStock {get; set;}
}