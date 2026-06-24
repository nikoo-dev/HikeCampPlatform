using System.ComponentModel.DataAnnotations;

namespace HikeCampPlatform.Api.Models;

public class Completion
{
    public int Id { get; set; }

    // Nullable -- a self-reported completion has no Booking at all
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }

    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public int TourId { get; set; }
    public Tour? Tour { get; set; }

    // Only set for operator-confirmed completions
    public int? ConfirmedByOperatorId { get; set; }
    public Operator? ConfirmedByOperator { get; set; }

    [Required]
    public bool IsSelfReported { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}