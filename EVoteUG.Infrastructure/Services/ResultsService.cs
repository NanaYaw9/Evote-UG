using EVoteUG.Core.DTOs.Results;
using EVoteUG.Core.Interfaces;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Infrastructure.Services;

public class ResultsService : IResultsService
{
    private readonly EVoteUGDbContext _context;

    public ResultsService(EVoteUGDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ElectionResultsResponseDto>> GetElectionResultsAsync(int electionId, bool isPrivilegedCaller = false)
    {
        var election = await _context.Elections
            .Include(e => e.Positions.OrderBy(p => p.OrderIndex))
                .ThenInclude(p => p.Candidates)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == electionId);

        if (election == null)
            return ApiResponse<ElectionResultsResponseDto>.Fail($"Election with ID {electionId} was not found.");

        // Embargo evaluation
        if (election.Status == ElectionStatus.Draft || election.Status == ElectionStatus.Scheduled)
        {
            return ApiResponse<ElectionResultsResponseDto>.Fail("Results are not available for draft or scheduled elections.");
        }

        if (election.Status == ElectionStatus.Active && !election.AllowRealtimeResults && !isPrivilegedCaller)
        {
            return ApiResponse<ElectionResultsResponseDto>.Fail("Live results for this election are currently embargoed by the Electoral Commission until the poll concludes.");
        }

        // Fetch all cast votes for this election
        var castVotes = await _context.CastVoteRecords
            .Where(v => v.ElectionId == electionId)
            .AsNoTracking()
            .ToListAsync();

        var totalBallotsCast = await _context.VoterParticipations
            .CountAsync(vp => vp.ElectionId == electionId);

        var totalEligibleVoters = await CountEligibleVotersAsync(election.Scope, election.ScopeTarget);

        var positionResults = new List<PositionResultDto>();

        foreach (var position in election.Positions)
        {
            var votesForPosition = castVotes.Where(v => v.PositionId == position.Id).ToList();
            var totalVotesForPos = votesForPosition.Count;

            var voteCountsByCandidate = votesForPosition
                .GroupBy(v => v.CandidateId)
                .ToDictionary(g => g.Key, g => g.Count());

            var tallies = new List<CandidateTallyDto>();

            foreach (var candidate in position.Candidates)
            {
                var count = voteCountsByCandidate.GetValueOrDefault(candidate.Id, 0);
                var percentage = totalVotesForPos > 0
                    ? Math.Round(((double)count / totalVotesForPos) * 100, 2)
                    : 0;

                tallies.Add(new CandidateTallyDto
                {
                    CandidateId = candidate.Id,
                    CandidateName = candidate.FullName,
                    Nickname = candidate.Nickname,
                    PhotoUrl = candidate.PhotoUrl,
                    VoteCount = count,
                    PercentageShare = percentage,
                    IsWinner = false
                });
            }

            // Sort tallies descending by vote count
            tallies = tallies.OrderByDescending(t => t.VoteCount).ToList();

            var winnerId = 0;
            var winnerName = "No votes cast";

            if (tallies.Count > 0 && tallies[0].VoteCount > 0)
            {
                tallies[0].IsWinner = true;
                winnerId = tallies[0].CandidateId;
                winnerName = tallies[0].CandidateName;
            }

            positionResults.Add(new PositionResultDto
            {
                PositionId = position.Id,
                PositionTitle = position.Title,
                TotalVotesForPosition = totalVotesForPos,
                WinnerCandidateId = winnerId,
                WinnerCandidateName = winnerName,
                Tallies = tallies
            });
        }

        var resultsDto = new ElectionResultsResponseDto
        {
            ElectionId = election.Id,
            ElectionTitle = election.Title,
            Status = election.Status,
            IsCertified = election.Status == ElectionStatus.Certified,
            TotalRegisteredVoters = totalEligibleVoters,
            TotalBallotsCast = totalBallotsCast,
            PositionResults = positionResults
        };

        return ApiResponse<ElectionResultsResponseDto>.Ok(resultsDto, "Election results tallied successfully.");
    }

    public async Task<ApiResponse<TurnoutAnalyticsDto>> GetTurnoutAnalyticsAsync(int electionId)
    {
        var election = await _context.Elections
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == electionId);

        if (election == null)
            return ApiResponse<TurnoutAnalyticsDto>.Fail($"Election with ID {electionId} was not found.");

        var eligibleStudentsQuery = GetEligibleStudentsQuery(election.Scope, election.ScopeTarget);
        var eligibleStudents = await eligibleStudentsQuery.AsNoTracking().ToListAsync();

        var participations = await _context.VoterParticipations
            .Include(vp => vp.Student)
            .Where(vp => vp.ElectionId == electionId)
            .AsNoTracking()
            .ToListAsync();

        var totalEligible = eligibleStudents.Count;
        var totalCast = participations.Count;
        var overallTurnout = totalEligible > 0
            ? Math.Round(((double)totalCast / totalEligible) * 100, 2)
            : 0;

        // Breakdown by Faculty
        var turnoutByFaculty = new Dictionary<string, DemographicTurnoutItemDto>();
        var facultyGroups = eligibleStudents
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Faculty) ? "Unassigned" : s.Faculty);

        foreach (var fg in facultyGroups)
        {
            var facultyName = fg.Key;
            var eligibleInFaculty = fg.Count();
            var votedInFaculty = participations.Count(p => p.Student != null && (string.IsNullOrWhiteSpace(p.Student.Faculty) ? "Unassigned" : p.Student.Faculty).Equals(facultyName, StringComparison.OrdinalIgnoreCase));

            turnoutByFaculty[facultyName] = new DemographicTurnoutItemDto
            {
                TotalEligible = eligibleInFaculty,
                VotesCast = votedInFaculty
            };
        }

        // Breakdown by Hall of Residence
        var turnoutByHall = new Dictionary<string, DemographicTurnoutItemDto>();
        var hallGroups = eligibleStudents
            .GroupBy(s => string.IsNullOrWhiteSpace(s.HallOfResidence) ? "Non-Resident" : s.HallOfResidence);

        foreach (var hg in hallGroups)
        {
            var hallName = hg.Key;
            var eligibleInHall = hg.Count();
            var votedInHall = participations.Count(p => p.Student != null && (string.IsNullOrWhiteSpace(p.Student.HallOfResidence) ? "Non-Resident" : p.Student.HallOfResidence).Equals(hallName, StringComparison.OrdinalIgnoreCase));

            turnoutByHall[hallName] = new DemographicTurnoutItemDto
            {
                TotalEligible = eligibleInHall,
                VotesCast = votedInHall
            };
        }

        // Breakdown by Level
        var turnoutByLevel = new Dictionary<string, DemographicTurnoutItemDto>();
        var levelGroups = eligibleStudents.GroupBy(s => s.Level);

        foreach (var lg in levelGroups)
        {
            var levelName = $"Level {lg.Key}";
            var eligibleInLevel = lg.Count();
            var votedInLevel = participations.Count(p => p.Student != null && p.Student.Level == lg.Key);

            turnoutByLevel[levelName] = new DemographicTurnoutItemDto
            {
                TotalEligible = eligibleInLevel,
                VotesCast = votedInLevel
            };
        }

        var analyticsDto = new TurnoutAnalyticsDto
        {
            ElectionId = electionId,
            TotalEligibleVoters = totalEligible,
            TotalVotesCast = totalCast,
            OverallTurnoutPercentage = overallTurnout,
            TurnoutByFaculty = turnoutByFaculty,
            TurnoutByHall = turnoutByHall,
            TurnoutByLevel = turnoutByLevel
        };

        return ApiResponse<TurnoutAnalyticsDto>.Ok(analyticsDto, "Turnout analytics computed successfully.");
    }

    private IQueryable<Student> GetEligibleStudentsQuery(ElectionScope scope, string scopeTarget)
    {
        var query = _context.Students.Where(s => s.IsActive && s.IsVerified);

        if (scope == ElectionScope.SRC || string.IsNullOrWhiteSpace(scopeTarget))
            return query;

        return scope switch
        {
            ElectionScope.HallOfResidence => query.Where(s => s.HallOfResidence == scopeTarget),
            ElectionScope.Faculty => query.Where(s => s.Faculty == scopeTarget),
            ElectionScope.Department => query.Where(s => s.Department == scopeTarget),
            ElectionScope.College => query.Where(s => s.College == scopeTarget),
            _ => query
        };
    }

    private async Task<int> CountEligibleVotersAsync(ElectionScope scope, string scopeTarget)
    {
        return await GetEligibleStudentsQuery(scope, scopeTarget).CountAsync();
    }
}
