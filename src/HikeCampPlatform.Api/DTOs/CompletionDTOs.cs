using System.ComponentModel.DataAnnotations;

namespace HikeCampPlatform.Api.DTOs;

public class SelfReportCompletionRequest
{
    [Required]
    public int TourId { get; set; }

    public DateTime? CompletedAt { get; set; } // optional -- defaults to now if not provided
}

public class CompletionResponse
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public int UserId { get; set; }
    public int TourId { get; set; }
    public string TourTitle { get; set; } = string.Empty;
    public bool IsSelfReported { get; set; }
    public DateTime CompletedAt { get; set; }
}