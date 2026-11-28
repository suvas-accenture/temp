namespace Capstone.API.DTOs.ServiceRequest;

public class RequestListItemDto
{
    public int RequestId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string RaisedBy { get; set; } = string.Empty;

    public DateTime RaisedOn { get; set; }

    public string StatusDescription { get; set; } = string.Empty;
}