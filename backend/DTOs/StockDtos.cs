using System.ComponentModel.DataAnnotations;

namespace InventoryApi.DTOs;

public class StockMoveRequest {
    [Range(1, int.MaxValue)]
    public int ProductId {get; set;}

    [Range(1, int.MaxValue)]
    public int Quantity {get; set;}

    [MaxLength(255)]
    public string? Note {get; set;}
}

public class StockMoveResponse {
    public int TransactionId {get; set;}
    public int ProductId {get; set;}
    public string ProductName {get; set;} = string.Empty;
    public string Type {get; set;} = string.Empty;
    public int Quantity {get; set;}
    public int BalanceAfter {get; set;}
    public string? Note {get; set;}
    public DateTime CreatedAt {get; set;}
}

public class StockTransactionResponse {
    public int Id {get; set;}
    public int ProductId {get; set;}
    public string ProductName {get; set;} = string.Empty;
    public string Type {get; set;} = string.Empty;
    public int Quantity {get; set;}
    public string? Note {get; set;}
    public DateTime CreatedAt {get; set;}
}