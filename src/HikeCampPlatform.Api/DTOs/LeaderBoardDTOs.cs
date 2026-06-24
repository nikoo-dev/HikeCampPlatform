namespace HikeCampPlatform.Api.DTOs;

public class LeaderboardEntryResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int CompletedTourCount { get; set; }
    public int Rank { get; set; }
}