namespace HikeCampPlatform.Api.Services;

public class LeaderboardEntry
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int CompletedTourCount { get; set; }
    public int Rank { get; set; }
}

public class CompletionRecord
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsSelfReported { get; set; }
}

public class LeaderboardService
{
    public List<LeaderboardEntry> BuildRankedLeaderboard(IEnumerable<CompletionRecord> completions)
    {
        var ranked = completions
            .Where(c => !c.IsSelfReported)
            .GroupBy(c => new { c.UserId, c.FullName })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.FullName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Select((entry, index) => new LeaderboardEntry
            {
                UserId = entry.UserId,
                FullName = entry.FullName,
                CompletedTourCount = entry.Count,
                Rank = index + 1
            })
            .ToList();

        return ranked;
    }
}