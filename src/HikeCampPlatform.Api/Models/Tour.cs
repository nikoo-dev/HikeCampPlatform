using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HikeCampPlatform.Api.Models;

public class Tour
{
    public int Id { get; set; }

    [Required]
    public int OperatorId { get; set; }

    // Navigation property: the operator who owns this tour
    public Operator? Operator { get; set; }

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
    [Column(TypeName = "decimal(10,2)")]
    public decimal PricePerPerson { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    [Required]
    public int MaxParticipants { get; set; }

    public bool IsPublished { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: one tour has many route points
    public List<TourRoutePoint> RoutePoints { get; set; } = new();  
}