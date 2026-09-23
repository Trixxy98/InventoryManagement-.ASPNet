using System.ComponentModel.DataAnnotations;

namespace InventoryApi.DTOs;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role {get; set;} = string.Empty;
}

public class RegisterRequest 
{
    [Required]
    [MaxLength(40)]
    public string Username {get; set;} = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password {get; set;} = string.Empty;
}
