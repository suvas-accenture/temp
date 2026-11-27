namespace Capstone.DAL.DTOs;

public class SummaryResponse
{
    public string UserName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int OpenRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int ClosedRequests { get; set; }
    public int ReOpenedRequests { get; set; }
    public List<RequestSummaryItem> Requests { get; set; } = new();
}