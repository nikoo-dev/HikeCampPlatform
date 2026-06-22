using System.ComponentModel.DataAnnotations;
using HikeCampPlatform.Api.Models;

namespace HikeCampPlatform.Api.DTOs;

public class RoutePointRequest
{
    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [Required]
    public int SequenceOrder { get; set; }

    [MaxLength(100)]
    public string? Label { get; set; }
}

public class CreateTourRequest
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TourType Type { get; set; }

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Region { get; set; }

    [Required]
    public DifficultyLevel DifficultyLevel { get; set; }

    [Required]
    public int DurationDays { get; set; }

    [Required]
    public decimal PricePerPerson { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    [Required]
    public int MaxParticipants { get; set; }

    public List<RoutePointRequest> RoutePoints { get; set; } = new();
}

public class TourResponse
{
    public int Id { get; set; }
    public int OperatorId { get; set; }
    public string OperatorCompanyName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TourType Type { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? Region { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public int DurationDays { get; set; }
    public decimal PricePerPerson { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public bool IsPublished { get; set; }
    public List<RoutePointRequest> RoutePoints { get; set; } = new();
}