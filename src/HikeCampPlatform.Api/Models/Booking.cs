using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HikeCampPlatform.Api.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public User? User { get; set; }

    [Required]
    public int TourDepartureId { get; set; }

    public TourDeparture? TourDeparture { get; set; }

    [Required]
    public int NumberOfParticipants { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;

    [MaxLength(255)]
    public string? StripePaymentIntentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}