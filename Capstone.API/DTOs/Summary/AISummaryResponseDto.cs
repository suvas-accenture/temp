namespace Capstone.API.DTOs.Summary;

public class AISummaryResponseDto
{
    public int TotalRequests { get; set; }

    public int OpenRequests { get; set; }

    public int ClosedRequests { get; set; }

    public string CommonIssues { get; set; } = string.Empty;

    public string OverallSummary { get; set; } = string.Empty;

    public string RecommendedNextActions { get; set; } = string.Empty;
}