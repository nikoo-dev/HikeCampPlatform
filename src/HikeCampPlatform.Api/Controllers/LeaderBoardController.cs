using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HikeCampPlatform.Api.Data;
using HikeCampPlatform.Api.DTOs;

namespace HikeCampPlatform.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public LeaderboardController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/leaderboard -- public
    [HttpGet]
    public async Task<ActionResult<List<LeaderboardEntryResponse>>> GetLeaderboard()
    {
        var grouped = await _db.Completions
            .Where(c => !c.IsSelfReported) // only verified completions count
            .Include(c => c.User)
            .GroupBy(c => new { c.UserId, c.User!.FullName })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.FullName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var response = grouped
            .Select((entry, index) => new LeaderboardEntryResponse
            {
                UserId = entry.UserId,
                FullName = entry.FullName,
                CompletedTourCount = entry.Count,
                Rank = index + 1
            })
            .ToList();

        return Ok(response);
    }
}