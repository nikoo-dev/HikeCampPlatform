using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HikeCampPlatform.Api.Data;
using HikeCampPlatform.Api.Models;
using HikeCampPlatform.Api.DTOs;

namespace HikeCampPlatform.Api.Controllers;

[ApiController]
[Route("api/tours")]
public class TourController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public TourController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // POST /api/tours -- operator only
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TourResponse>> CreateTour([FromBody] CreateTourRequest request)
    {
        // Confirm the caller is an Operator, not a User
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

        var operatorEntity = await _db.Operators.FindAsync(operatorId);
        if (operatorEntity == null)
        {
            return Unauthorized();
        }

        var tour = new Tour
        {
            OperatorId = operatorId,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            Country = request.Country,
            Region = request.Region,
            DifficultyLevel = request.DifficultyLevel,
            DurationDays = request.DurationDays,
            PricePerPerson = request.PricePerPerson,
            Currency = request.Currency,
            MaxParticipants = request.MaxParticipants,
            IsPublished = false, // becomes true only after listing fee + approval -- handled in a later step
            RoutePoints = request.RoutePoints.Select(rp => new TourRoutePoint
            {
                Latitude = rp.Latitude,
                Longitude = rp.Longitude,
                SequenceOrder = rp.SequenceOrder,
                Label = rp.Label
            }).ToList()
        };

        _db.Tours.Add(tour);
        await _db.SaveChangesAsync();

        return Ok(MapToResponse(tour, operatorEntity.CompanyName));
    }

    // GET /api/tours -- public, only published tours
    [HttpGet]
    public async Task<ActionResult<List<TourResponse>>> GetTours()
    {
        var tours = await _db.Tours
            .Include(t => t.Operator)
            .Include(t => t.RoutePoints)
            .Where(t => t.IsPublished)
            .ToListAsync();

        var response = tours.Select(t => MapToResponse(t, t.Operator?.CompanyName ?? "")).ToList();
        return Ok(response);
    }

    // GET /api/tours/{id} -- public, single tour
    [HttpGet("{id}")]
    public async Task<ActionResult<TourResponse>> GetTourById(int id)
    {
        var tour = await _db.Tours
            .Include(t => t.Operator)
            .Include(t => t.RoutePoints)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tour == null || !tour.IsPublished)
        {
            return NotFound();
        }

        return Ok(MapToResponse(tour, tour.Operator?.CompanyName ?? ""));
    }

    // PATCH /api/tours/{id}/publish -- admin only (temporary hardcoded key check)
    [HttpPatch("{id}/publish")]
    public async Task<ActionResult> PublishTour(int id, [FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        var expectedKey = _config["AdminSettings:AdminKey"];

        if (string.IsNullOrEmpty(adminKey) || adminKey != expectedKey)
        {
            return Unauthorized(new { message = "Invalid admin key." });
        }

        var tour = await _db.Tours.FindAsync(id);
        if (tour == null)
        {
            return NotFound();
        }

        tour.IsPublished = true;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Tour published.", tourId = tour.Id, isPublished = tour.IsPublished });
    }

    private static TourResponse MapToResponse(Tour tour, string operatorCompanyName)
    {
        return new TourResponse
        {
            Id = tour.Id,
            OperatorId = tour.OperatorId,
            OperatorCompanyName = operatorCompanyName,
            Title = tour.Title,
            Description = tour.Description,
            Type = tour.Type,
            Country = tour.Country,
            Region = tour.Region,
            DifficultyLevel = tour.DifficultyLevel,
            DurationDays = tour.DurationDays,
            PricePerPerson = tour.PricePerPerson,
            Currency = tour.Currency,
            MaxParticipants = tour.MaxParticipants,
            IsPublished = tour.IsPublished,
            RoutePoints = tour.RoutePoints
                .OrderBy(rp => rp.SequenceOrder)
                .Select(rp => new RoutePointRequest
                {
                    Latitude = rp.Latitude,
                    Longitude = rp.Longitude,
                    SequenceOrder = rp.SequenceOrder,
                    Label = rp.Label
                }).ToList()
        };
    }
}