using EVoteUG.Infrastructure.Services;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Tests.Helpers;
using Xunit;

namespace EVoteUG.Tests.Services;

public class ResultsServiceTests
{
    [Fact]
    public async Task GetElectionResults_CalculatesTalliesAndIdentifiesWinnerCorrectly()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var candidateA = new Candidate { Id = 1, FullName = "Alice", Status = CandidateStatus.Approved };
        var candidateB = new Candidate { Id = 2, FullName = "Bob", Status = CandidateStatus.Approved };

        var position = new Position
        {
            Id = 1,
            Title = "SRC President",
            Candidates = new List<Candidate> { candidateA, candidateB }
        };

        var election = new Election
        {
            Id = 1,
            Title = "2026 Concluded Election",
            Status = ElectionStatus.Concluded,
            Positions = new List<Position> { position }
        };

        context.Elections.Add(election);

        // Seed CastVoteRecords: Alice gets 3 votes, Bob gets 1 vote
        context.CastVoteRecords.AddRange(new List<CastVoteRecord>
        {
            new() { ElectionId = 1, PositionId = 1, CandidateId = 1 },
            new() { ElectionId = 1, PositionId = 1, CandidateId = 1 },
            new() { ElectionId = 1, PositionId = 1, CandidateId = 1 },
            new() { ElectionId = 1, PositionId = 1, CandidateId = 2 }
        });

        // Seed 4 voter participations
        context.VoterParticipations.AddRange(new List<VoterParticipation>
        {
            new() { ElectionId = 1, StudentId = 101 },
            new() { ElectionId = 1, StudentId = 102 },
            new() { ElectionId = 1, StudentId = 103 },
            new() { ElectionId = 1, StudentId = 104 }
        });

        await context.SaveChangesAsync();

        var resultsService = new ResultsService(context);

        // Act
        var result = await resultsService.GetElectionResultsAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(4, result.Data.TotalBallotsCast);

        var posResult = Assert.Single(result.Data.PositionResults);
        Assert.Equal(4, posResult.TotalVotesForPosition);
        Assert.Equal(1, posResult.WinnerCandidateId); // Alice is winner
        Assert.Equal("Alice", posResult.WinnerCandidateName);

        var aliceTally = posResult.Tallies.First(t => t.CandidateId == 1);
        Assert.Equal(3, aliceTally.VoteCount);
        Assert.Equal(75.0, aliceTally.PercentageShare);
        Assert.True(aliceTally.IsWinner);

        var bobTally = posResult.Tallies.First(t => t.CandidateId == 2);
        Assert.Equal(1, bobTally.VoteCount);
        Assert.Equal(25.0, bobTally.PercentageShare);
        Assert.False(bobTally.IsWinner);
    }

    [Fact]
    public async Task GetElectionResults_WhenActiveAndNoRealtimeAllowed_EmbargoesResultsForNonPrivileged()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var election = new Election
        {
            Id = 1,
            Title = "Active Embargoed Election",
            Status = ElectionStatus.Active,
            AllowRealtimeResults = false // Embargoed
        };
        context.Elections.Add(election);
        await context.SaveChangesAsync();

        var resultsService = new ResultsService(context);

        // Act (non-privileged student request)
        var result = await resultsService.GetElectionResultsAsync(1, isPrivilegedCaller: false);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("embargoed", result.Message);
    }
}
