using Xunit;
using HikeCampPlatform.Api.Services;

namespace HikeCampPlatform.Tests;

public class LeaderboardServiceTests
{
    private readonly LeaderboardService _sut;

    public LeaderboardServiceTests()
    {
        _sut = new LeaderboardService();
    }

    [Fact]
    public void BuildRankedLeaderboard_ExcludesSelfReportedCompletions()
    {
        var completions = new List<CompletionRecord>
        {
            new() { UserId = 1, FullName = "Jane", IsSelfReported = false },
            new() { UserId = 1, FullName = "Jane", IsSelfReported = true }, // should NOT count
        };

        var result = _sut.BuildRankedLeaderboard(completions);

        Assert.Single(result);
        Assert.Equal(1, result[0].CompletedTourCount);
    }

    [Fact]
    public void BuildRankedLeaderboard_GroupsCompletionsByUser()
    {
        var completions = new List<CompletionRecord>
        {
            new() { UserId = 1, FullName = "Jane", IsSelfReported = false },
            new() { UserId = 1, FullName = "Jane", IsSelfReported = false },
            new() { UserId = 1, FullName = "Jane", IsSelfReported = false },
        };

        var result = _sut.BuildRankedLeaderboard(completions);

        Assert.Single(result);
        Assert.Equal(3, result[0].CompletedTourCount);
    }

    [Fact]
    public void BuildRankedLeaderboard_OrdersByCountDescending()
    {
        var completions = new List<CompletionRecord>
        {
            new() { UserId = 1, FullName = "Jane", IsSelfReported = false },
            new() { UserId = 2, FullName = "Mark", IsSelfReported = false },
            new() { UserId = 2, FullName = "Mark", IsSelfReported = false },
            new() { UserId = 2, FullName = "Mark", IsSelfReported = false },
        };

        var result = _sut.BuildRankedLeaderboard(completions);

        Assert.Equal(2, result[0].UserId);   // Mark (3 completions) ranked first
        Assert.Equal(1, result[0].Rank);
        Assert.Equal(1, result[1].UserId);   // Jane (1 completion) ranked second
        Assert.Equal(2, result[1].Rank);
    }

    [Fact]
    public void BuildRankedLeaderboard_NoCompletions_ReturnsEmptyList()
    {
        var completions = new List<CompletionRecord>();

        var result = _sut.BuildRankedLeaderboard(completions);

        Assert.Empty(result);
    }

    [Fact]
    public void BuildRankedLeaderboard_OnlySelfReportedCompletions_ReturnsEmptyList()
    {
        var completions = new List<CompletionRecord>
        {
            new() { UserId = 1, FullName = "Jane", IsSelfReported = true },
        };

        var result = _sut.BuildRankedLeaderboard(completions);

        Assert.Empty(result);
    }
}