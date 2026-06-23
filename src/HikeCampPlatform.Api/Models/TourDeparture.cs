using System.ComponentModel.DataAnnotations;

namespace HikeCampPlatform.Api.Models;

public class TourDeparture
{
    public int Id { get; set; }

    [Required]
    public int TourId { get; set; }

    public Tour? Tour { get; set; }

    [Required]
    public DateTime DepartureDate { get; set; }

    [Required]
    public int MaxParticipants { get; set; }

    public int CurrentParticipants { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Booking> Bookings { get; set; } = new();
}