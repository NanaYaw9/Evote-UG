using EVoteUG.Core.DTOs.Voting;
using EVoteUG.Core.Interfaces;
using EVoteUG.Core.Validators;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Infrastructure.Security;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Infrastructure.Services;

public class VotingService : IVotingService
{
    private readonly EVoteUGDbContext _context;
    private readonly IAuditService _auditService;
    private readonly CastBallotValidator _ballotValidator;

    public VotingService(EVoteUGDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
        _ballotValidator = new CastBallotValidator();
    }

    public async Task<ApiResponse<BallotResponseDto>> GetEligibleBallotAsync(int electionId, int studentId)
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null || !student.IsActive)
            return ApiResponse<BallotResponseDto>.Fail("Student voter account not found or is inactive.");

        var election = await _context.Elections
            .Include(e => e.Positions.OrderBy(p => p.OrderIndex))
                .ThenInclude(p => p.Candidates.Where(c => c.Status == CandidateStatus.Approved))
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == electionId);

        if (election == null)
            return ApiResponse<BallotResponseDto>.Fail($"Election with ID {electionId} was not found.");

        // Validate election status and dates
        if (election.Status != ElectionStatus.Active || DateTime.UtcNow < election.StartDate || DateTime.UtcNow > election.EndDate)
        {
            return ApiResponse<BallotResponseDto>.Fail("This election is not currently open for voting.");
        }

        // Validate demographic scope eligibility
        if (!IsStudentEligibleForScope(student, election.Scope, election.ScopeTarget))
        {
            return ApiResponse<BallotResponseDto>.Fail($"You are not eligible to vote in this election (Scope: {election.Scope} - {election.ScopeTarget}).");
        }

        // Check if student has already voted
        var hasVoted = await _context.VoterParticipations
            .AnyAsync(vp => vp.StudentId == studentId && vp.ElectionId == electionId);

        var ballotDto = new BallotResponseDto
        {
            ElectionId = election.Id,
            ElectionTitle = election.Title,
            AcademicYear = election.AcademicYear,
            StartDate = election.StartDate,
            EndDate = election.EndDate,
            HasVoted = hasVoted,
            Positions = election.Positions.Select(p => new PositionBallotItemDto
            {
                PositionId = p.Id,
                Title = p.Title,
                Description = p.Description,
                MaxVotesAllowed = p.MaxVotesAllowed,
                Candidates = p.Candidates.Select(c => new CandidateBallotItemDto
                {
                    CandidateId = c.Id,
                    FullName = c.FullName,
                    Nickname = c.Nickname,
                    Bio = c.Bio,
                    PhotoUrl = c.PhotoUrl,
                    ManifestoUrl = c.ManifestoUrl,
                    RunningMateName = c.RunningMateName,
                    RunningMatePhotoUrl = c.RunningMatePhotoUrl
                }).ToList()
            }).ToList()
        };

        return ApiResponse<BallotResponseDto>.Ok(ballotDto, "Ballot retrieved successfully.");
    }

    public async Task<ApiResponse<VoterStatusResponseDto>> CheckVoterStatusAsync(int electionId, int studentId)
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
            return ApiResponse<VoterStatusResponseDto>.Fail("Student voter account not found.");

        var election = await _context.Elections
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == electionId);

        if (election == null)
            return ApiResponse<VoterStatusResponseDto>.Fail("Election not found.");

        var isEligible = IsStudentEligibleForScope(student, election.Scope, election.ScopeTarget);

        var participation = await _context.VoterParticipations
            .AsNoTracking()
            .FirstOrDefaultAsync(vp => vp.StudentId == studentId && vp.ElectionId == electionId);

        var receipt = await _context.VoteReceipts
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.StudentId == studentId && r.ElectionId == electionId);

        var statusDto = new VoterStatusResponseDto
        {
            ElectionId = electionId,
            IsEligible = isEligible,
            HasVoted = participation != null,
            CastAt = participation?.CastAt,
            ReceiptHash = receipt?.ReceiptHash
        };

        return ApiResponse<VoterStatusResponseDto>.Ok(statusDto, "Voter status retrieved.");
    }

    public async Task<ApiResponse<VoteReceiptResponseDto>> CastBallotAsync(
        int studentId, 
        CastBallotRequestDto request, 
        string ipAddress, 
        string deviceInfo = "")
    {
        // 1. Validate payload structure
        var validation = await _ballotValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<VoteReceiptResponseDto>.Fail(
                "Invalid ballot submission.",
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        // 2. Begin Atomic Database Transaction
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null || !student.IsActive)
            {
                return ApiResponse<VoteReceiptResponseDto>.Fail("Student voter profile is invalid or deactivated.");
            }

            var election = await _context.Elections
                .Include(e => e.Positions)
                    .ThenInclude(p => p.Candidates)
                .FirstOrDefaultAsync(e => e.Id == request.ElectionId);

            if (election == null)
            {
                return ApiResponse<VoteReceiptResponseDto>.Fail("Election not found.");
            }

            if (election.Status != ElectionStatus.Active || DateTime.UtcNow < election.StartDate || DateTime.UtcNow > election.EndDate)
            {
                return ApiResponse<VoteReceiptResponseDto>.Fail("This election is not currently open for ballot submission.");
            }

            if (!IsStudentEligibleForScope(student, election.Scope, election.ScopeTarget))
            {
                return ApiResponse<VoteReceiptResponseDto>.Fail("You are not eligible to vote in this election.");
            }

            // 3. Strict Double-Voting Check
            var alreadyVoted = await _context.VoterParticipations
                .AnyAsync(vp => vp.StudentId == studentId && vp.ElectionId == request.ElectionId);

            if (alreadyVoted)
            {
                return ApiResponse<VoteReceiptResponseDto>.Fail("You have already cast your ballot in this election. Multiple voting is strictly prohibited.");
            }

            // 4. Validate Selections against Election Positions & Candidates
            var electionPositionMap = election.Positions.ToDictionary(p => p.Id);
            foreach (var selection in request.Selections)
            {
                if (!electionPositionMap.TryGetValue(selection.PositionId, out var position))
                {
                    return ApiResponse<VoteReceiptResponseDto>.Fail($"Position #{selection.PositionId} does not belong to this election.");
                }

                var candidate = position.Candidates.FirstOrDefault(c => c.Id == selection.CandidateId && c.Status == CandidateStatus.Approved);
                if (candidate == null)
                {
                    return ApiResponse<VoteReceiptResponseDto>.Fail($"Invalid candidate selected for position '{position.Title}'.");
                }
            }

            // 5. Insert VoterParticipation (WHO voted - no candidate choices)
            var participation = new VoterParticipation
            {
                StudentId = studentId,
                ElectionId = request.ElectionId,
                CastAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                DeviceInfo = deviceInfo
            };
            _context.VoterParticipations.Add(participation);

            // 6. Insert CastVoteRecords (WHAT was voted - decoupled with zero student FK)
            var batchId = Guid.NewGuid().ToString("N");
            var castTimestamp = DateTime.UtcNow;

            foreach (var selection in request.Selections)
            {
                var record = new CastVoteRecord
                {
                    ElectionId = request.ElectionId,
                    PositionId = selection.PositionId,
                    CandidateId = selection.CandidateId,
                    CastTimestamp = castTimestamp,
                    BallotBatchId = batchId
                };
                _context.CastVoteRecords.Add(record);
            }

            // 7. Generate Cryptographic SHA-256 Digital Receipt
            var receiptHash = ReceiptHasher.GenerateReceiptHash(studentId, request.ElectionId, castTimestamp);
            var receipt = new VoteReceipt
            {
                StudentId = studentId,
                ElectionId = request.ElectionId,
                ReceiptHash = receiptHash,
                IssuedAt = castTimestamp
            };
            _context.VoteReceipts.Add(receipt);

            // 8. Commit Transaction
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 9. Audit Logging (Preserves voter privacy while confirming participation)
            await _auditService.LogActionAsync(
                null,
                AuditEventType.BallotSubmission,
                $"Student ID {student.StudentId} cast ballot in Election #{election.Id}",
                "VoterParticipation",
                participation.Id,
                new { ElectionId = election.Id, ReceiptHash = receiptHash },
                ipAddress);

            var receiptDto = new VoteReceiptResponseDto
            {
                ElectionId = election.Id,
                ElectionTitle = election.Title,
                ReceiptHash = receiptHash,
                Timestamp = castTimestamp,
                Message = "Your vote has been cast and cryptographically recorded. Keep this digital receipt for verification."
            };

            return ApiResponse<VoteReceiptResponseDto>.Ok(receiptDto, "Ballot successfully cast.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<VoteReceiptResponseDto>.Fail($"Ballot submission failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<VoteReceiptResponseDto>>> GetStudentReceiptsAsync(int studentId)
    {
        var receipts = await _context.VoteReceipts
            .Include(r => r.Election)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.IssuedAt)
            .AsNoTracking()
            .ToListAsync();

        var dtos = receipts.Select(r => new VoteReceiptResponseDto
        {
            ElectionId = r.ElectionId,
            ElectionTitle = r.Election != null ? r.Election.Title : $"Election #{r.ElectionId}",
            ReceiptHash = r.ReceiptHash,
            Timestamp = r.IssuedAt,
            Message = "Cryptographic proof of voting."
        }).ToList();

        return ApiResponse<List<VoteReceiptResponseDto>>.Ok(dtos, "Vote receipts retrieved successfully.");
    }

    public async Task<ApiResponse<bool>> VerifyReceiptHashAsync(string receiptHash)
    {
        if (string.IsNullOrWhiteSpace(receiptHash))
            return ApiResponse<bool>.Fail("Receipt hash cannot be empty.");

        var exists = await _context.VoteReceipts
            .AnyAsync(r => r.ReceiptHash.ToUpper() == receiptHash.Trim().ToUpper());

        if (exists)
        {
            return ApiResponse<bool>.Ok(true, "Digital vote receipt is authentic and recorded on the platform.");
        }

        return ApiResponse<bool>.Fail("Digital receipt hash not found in the verified ledger.");
    }

    private static bool IsStudentEligibleForScope(Student student, ElectionScope scope, string scopeTarget)
    {
        if (scope == ElectionScope.SRC)
            return true;

        if (string.IsNullOrWhiteSpace(scopeTarget))
            return true;

        return scope switch
        {
            ElectionScope.HallOfResidence => student.HallOfResidence.Equals(scopeTarget, StringComparison.OrdinalIgnoreCase),
            ElectionScope.Faculty => student.Faculty.Equals(scopeTarget, StringComparison.OrdinalIgnoreCase),
            ElectionScope.Department => student.Department.Equals(scopeTarget, StringComparison.OrdinalIgnoreCase),
            ElectionScope.College => student.College.Equals(scopeTarget, StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }
}
