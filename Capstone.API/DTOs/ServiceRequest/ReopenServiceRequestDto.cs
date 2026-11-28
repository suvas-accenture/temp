using System.ComponentModel.DataAnnotations;

namespace Capstone.API.DTOs.ServiceRequest;

public class ReopenServiceRequestDto
{
    [Range(1, int.MaxValue)]
    public int RequestId { get; set; }
}