using EVoteUG.Core.DTOs.Auth;
using EVoteUG.Core.DTOs.Election;
using EVoteUG.Core.DTOs.Voting;
using EVoteUG.Core.Validators;
using Xunit;

namespace EVoteUG.Tests.Validators;

public class ValidatorsTests
{
    [Fact]
    public async Task StudentLoginValidator_ValidStudentId_PassesValidation()
    {
        // Arrange
        var validator = new StudentLoginValidator();
        var request = new StudentLoginRequestDto
        {
            StudentId = "10987654",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("abc1234567")]
    public async Task StudentLoginValidator_InvalidStudentId_FailsValidation(string invalidStudentId)
    {
        // Arrange
        var validator = new StudentLoginValidator();
        var request = new StudentLoginRequestDto
        {
            StudentId = invalidStudentId,
            Password = "SecurePassword123!"
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(StudentLoginRequestDto.StudentId));
    }

    [Fact]
    public async Task CreateElectionValidator_EndDateBeforeStartDate_FailsValidation()
    {
        // Arrange
        var validator = new CreateElectionValidator();
        var request = new CreateElectionRequestDto
        {
            Title = "SRC General Elections",
            AcademicYear = "2026/2027",
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(1) // End date before start date
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateElectionRequestDto.EndDate));
    }

    [Fact]
    public async Task CastBallotValidator_DuplicatePositionInSameBallot_FailsValidation()
    {
        // Arrange
        var validator = new CastBallotValidator();
        var request = new CastBallotRequestDto
        {
            ElectionId = 1,
            Selections = new List<PositionVoteSelectionDto>
            {
                new() { PositionId = 1, CandidateId = 10 },
                new() { PositionId = 1, CandidateId = 11 } // Duplicate PositionId 1
            }
        };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("multiple votes for the same position"));
    }
}
