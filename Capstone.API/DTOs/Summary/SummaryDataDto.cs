namespace Capstone.API.DTOs.Summary;

public class SummaryDataDto
{
    public string UserName { get; set; } = string.Empty;

    public int TotalRequests { get; set; }

    public int OpenRequests { get; set; }

    public int InProgressRequests { get; set; }

    public int ClosedRequests { get; set; }

    public int ReOpenedRequests { get; set; }

    public List<RequestSummaryItemDto> Requests { get; set; } = new();
}