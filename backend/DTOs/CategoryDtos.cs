using System.ComponentModel.DataAnnotations;

namespace InventoryApi.DTOs;

public class CategoryResponse {
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string? Description {get; set;}
    public DateTime CreatedAt {get; set;}
    public int ProductCount {get; set;}
}

public class CategoryRequest {
    [Required]
    [MaxLength(80)]
    public string Name {get; set;} = string.Empty;

    [MaxLength(255)]
    public string? Description {get; set;}
}