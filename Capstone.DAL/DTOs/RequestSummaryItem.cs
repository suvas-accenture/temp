namespace Capstone.DAL.DTOs;

public class RequestSummaryItem
{
    public int RequestId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RaisedOn { get; set; }
}