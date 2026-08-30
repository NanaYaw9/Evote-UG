using EVoteUG.Core.DTOs.Voting;
using EVoteUG.Infrastructure.Services;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EVoteUG.Tests.Services;

public class VotingServiceTests
{
    [Fact]
    public async Task CastBallot_FirstTime_SuccessfullyRecordsParticipationAndReturnsReceipt()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var auditServiceMock = new Mock<Core.Interfaces.IAuditService>();

        var student = new Student
        {
            StudentId = "10987654",
            FullName = "Ama Serwaa",
            Email = "aserwaa@st.ug.edu.gh",
            IsVerified = true,
            IsActive = true
        };
        context.Students.Add(student);

        var candidate = new Candidate
        {
            Id = 1,
            FullName = "Candidate John",
            Status = CandidateStatus.Approved
        };

        var position = new Position
        {
            Id = 1,
            Title = "SRC President",
            MaxVotesAllowed = 1,
            Candidates = new List<Candidate> { candidate }
        };

        var election = new Election
        {
            Id = 1,
            Title = "2026 UG SRC Elections",
            Scope = ElectionScope.SRC,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = ElectionStatus.Active,
            Positions = new List<Position> { position }
        };
        context.Elections.Add(election);
        await context.SaveChangesAsync();

        var votingService = new VotingService(context, auditServiceMock.Object);

        var request = new CastBallotRequestDto
        {
            ElectionId = 1,
            Selections = new List<PositionVoteSelectionDto>
            {
                new() { PositionId = 1, CandidateId = 1 }
            }
        };

        // Act
        var result = await votingService.CastBallotAsync(student.Id, request, "127.0.0.1", "Chrome");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.ReceiptHash);

        // Verify WHO voted was recorded
        var participation = await context.VoterParticipations.FirstOrDefaultAsync(vp => vp.StudentId == student.Id);
        Assert.NotNull(participation);

        // Verify WHAT was voted was recorded (anonymized)
        var castRecords = await context.CastVoteRecords.Where(r => r.ElectionId == 1).ToListAsync();
        Assert.Single(castRecords);
        Assert.Equal(1, castRecords[0].CandidateId);

        // Verify Receipt recorded
        var receipt = await context.VoteReceipts.FirstOrDefaultAsync(r => r.StudentId == student.Id);
        Assert.NotNull(receipt);
        Assert.Equal(result.Data.ReceiptHash, receipt.ReceiptHash);
    }

    [Fact]
    public async Task CastBallot_SecondTime_StrictlyRejectsDoubleVoting()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var auditServiceMock = new Mock<Core.Interfaces.IAuditService>();

        var student = new Student { Id = 1, StudentId = "10987654", IsActive = true, IsVerified = true };
        var candidate = new Candidate { Id = 1, Status = CandidateStatus.Approved };
        var position = new Position { Id = 1, Candidates = new List<Candidate> { candidate } };
        var election = new Election
        {
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = ElectionStatus.Active,
            Positions = new List<Position> { position }
        };

        context.Students.Add(student);
        context.Elections.Add(election);

        // Simulate that the student already voted previously
        context.VoterParticipations.Add(new VoterParticipation
        {
            StudentId = 1,
            ElectionId = 1,
            CastAt = DateTime.UtcNow.AddHours(-1)
        });
        await context.SaveChangesAsync();

        var votingService = new VotingService(context, auditServiceMock.Object);

        var request = new CastBallotRequestDto
        {
            ElectionId = 1,
            Selections = new List<PositionVoteSelectionDto>
            {
                new() { PositionId = 1, CandidateId = 1 }
            }
        };

        // Act
        var result = await votingService.CastBallotAsync(student.Id, request, "127.0.0.1", "Chrome");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already cast your ballot", result.Message);
    }
}
