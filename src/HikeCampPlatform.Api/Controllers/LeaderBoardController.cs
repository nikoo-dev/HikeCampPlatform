using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HikeCampPlatform.Api.Data;
using HikeCampPlatform.Api.Services;

namespace HikeCampPlatform.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly LeaderboardService _leaderboardService;

    public LeaderboardController(AppDbContext db, LeaderboardService leaderboardService)
    {
        _db = db;
        _leaderboardService = leaderboardService;
    }

    // GET /api/leaderboard -- public
    [HttpGet]
    public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard()
    {
        var rawCompletions = await _db.Completions
            .Include(c => c.User)
            .Select(c => new CompletionRecord
            {
                UserId = c.UserId,
                FullName = c.User!.FullName,
                IsSelfReported = c.IsSelfReported
            })
            .ToListAsync();

        var leaderboard = _leaderboardService.BuildRankedLeaderboard(rawCompletions);

        return Ok(leaderboard);
    }
}