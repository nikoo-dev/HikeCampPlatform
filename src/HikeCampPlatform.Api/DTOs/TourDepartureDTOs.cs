using System.ComponentModel.DataAnnotations;

namespace HikeCampPlatform.Api.DTOs;

public class CreateDepartureRequest
{
    [Required]
    public DateTime DepartureDate { get; set; }

    public int? MaxParticipants { get; set; } // optional override; defaults to Tour's MaxParticipants if not provided
}

public class DepartureResponse
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public DateTime DepartureDate { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public int SpotsRemaining => MaxParticipants - CurrentParticipants;
}