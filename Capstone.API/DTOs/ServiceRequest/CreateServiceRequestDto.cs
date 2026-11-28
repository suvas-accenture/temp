using System.ComponentModel.DataAnnotations;

namespace Capstone.API.DTOs.ServiceRequest;

public class CreateServiceRequestDto
{
    [Required]
    [StringLength(50)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Details { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Justification { get; set; } = string.Empty;
}