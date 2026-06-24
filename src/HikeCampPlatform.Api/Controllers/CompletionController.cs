using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HikeCampPlatform.Api.Data;
using HikeCampPlatform.Api.Models;
using HikeCampPlatform.Api.DTOs;

namespace HikeCampPlatform.Api.Controllers;

[ApiController]
[Route("api/completions")]
public class CompletionController : ControllerBase
{
    private readonly AppDbContext _db;

    public CompletionController(AppDbContext db)
    {
        _db = db;
    }

    // POST /api/completions/self-report -- User only
    [HttpPost("self-report")]
    [Authorize]
    public async Task<ActionResult<CompletionResponse>> SelfReport([FromBody] SelfReportCompletionRequest request)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "User")
        {
            return Forbid();
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var tour = await _db.Tours.FindAsync(request.TourId);
        if (tour == null)
        {
            return NotFound(new { message = "Tour not found." });
        }

        var completion = new Completion
        {
            BookingId = null,
            UserId = userId,
            TourId = tour.Id,
            ConfirmedByOperatorId = null,
            IsSelfReported = true,
            CompletedAt = request.CompletedAt ?? DateTime.UtcNow
        };

        _db.Completions.Add(completion);
        await _db.SaveChangesAsync();

        return Ok(new CompletionResponse
        {
            Id = completion.Id,
            BookingId = completion.BookingId,
            UserId = completion.UserId,
            TourId = completion.TourId,
            TourTitle = tour.Title,
            IsSelfReported = completion.IsSelfReported,
            CompletedAt = completion.CompletedAt
        });
    }
}