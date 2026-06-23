using System.ComponentModel.DataAnnotations;
using HikeCampPlatform.Api.Models;

namespace HikeCampPlatform.Api.DTOs;

public class CreateBookingRequest
{
    [Required]
    public int TourDepartureId { get; set; }

    [Required]
    [Range(1, 50)]
    public int NumberOfParticipants { get; set; }
}

public class BookingResponse
{
    public int Id { get; set; }
    public int TourDepartureId { get; set; }
    public int NumberOfParticipants { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public string ClientSecret { get; set; } = string.Empty; // needed by frontend to confirm payment
}