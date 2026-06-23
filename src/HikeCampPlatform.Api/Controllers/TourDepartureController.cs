using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HikeCampPlatform.Api.Data;
using HikeCampPlatform.Api.Models;
using HikeCampPlatform.Api.DTOs;

namespace HikeCampPlatform.Api.Controllers;

[ApiController]
[Route("api/tours/{tourId}/departures")]
public class TourDepartureController : ControllerBase
{
    private readonly AppDbContext _db;

    public TourDepartureController(AppDbContext db)
    {
        _db = db;
    }

    // POST /api/tours/{tourId}/departures -- operator only, must own the tour
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<DepartureResponse>> CreateDeparture(int tourId, [FromBody] CreateDepartureRequest request)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Operator")
        {
            return Forbid();
        }

        var operatorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (operatorIdClaim == null || !int.TryParse(operatorIdClaim, out var operatorId))
        {
            return Unauthorized();
        }

        var tour = await _db.Tours.FindAsync(tourId);
        if (tour == null)
        {
            return NotFound(new { message = "Tour not found." });
        }

        // Confirm this operator actually owns the tour they're adding a departure to
        if (tour.OperatorId != operatorId)
        {
            return Forbid();
        }

        var departure = new TourDeparture
        {
            TourId = tourId,
            DepartureDate = request.DepartureDate,
            MaxParticipants = request.MaxParticipants ?? tour.MaxParticipants,
            CurrentParticipants = 0
        };

        _db.TourDepartures.Add(departure);
        await _db.SaveChangesAsync();

        return Ok(MapToResponse(departure));
    }

    // GET /api/tours/{tourId}/departures -- public, list upcoming departures for a tour
    [HttpGet]
    public async Task<ActionResult<List<DepartureResponse>>> GetDepartures(int tourId)
    {
        var departures = await _db.TourDepartures
            .Where(d => d.TourId == tourId && d.DepartureDate > DateTime.UtcNow)
            .OrderBy(d => d.DepartureDate)
            .ToListAsync();

        return Ok(departures.Select(MapToResponse).ToList());
    }

    private static DepartureResponse MapToResponse(TourDeparture d)
    {
        return new DepartureResponse
        {
            Id = d.Id,
            TourId = d.TourId,
            DepartureDate = d.DepartureDate,
            MaxParticipants = d.MaxParticipants,
            CurrentParticipants = d.CurrentParticipants
        };
    }
}