using System.ComponentModel.DataAnnotations;

namespace HikeCampPlatform.Api.Models;

public class TourRoutePoint
{
    public int Id { get; set; }

    [Required]
    public int TourId { get; set; }

    // Navigation property back to the parent tour
    public Tour? Tour { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [Required]
    public int SequenceOrder { get; set; }

    [MaxLength(100)]
    public string? Label { get; set; }
}