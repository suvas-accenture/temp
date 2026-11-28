using System.ComponentModel.DataAnnotations;

namespace Capstone.API.DTOs.Authentication;

public class LoginRequestDto
{
    [Required]
    [StringLength(20)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}