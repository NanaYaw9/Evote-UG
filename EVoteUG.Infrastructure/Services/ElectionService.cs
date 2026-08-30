using EVoteUG.Core.DTOs.Candidate;
using EVoteUG.Core.DTOs.Election;
using EVoteUG.Core.DTOs.Position;
using EVoteUG.Core.Interfaces;
using EVoteUG.Core.Validators;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Infrastructure.Services;

public class ElectionService : IElectionService
{
    private readonly EVoteUGDbContext _context;
    private readonly IAuditService _auditService;
    private readonly CreateElectionValidator _createValidator;

    public ElectionService(EVoteUGDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
        _createValidator = new CreateElectionValidator();
    }

    public async Task<ApiResponse<List<ElectionResponseDto>>> GetElectionsAsync(ElectionStatus? status = null, ElectionScope? scope = null)
    {
        var query = _context.Elections
            .Include(e => e.Positions)
                .ThenInclude(p => p.Candidates)
            .Include(e => e.VoterParticipations)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (scope.HasValue)
        {
            query = query.Where(e => e.Scope == scope.Value);
        }

        var elections = await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var dtoList = elections.Select(MapToResponseDto).ToList();

        return ApiResponse<List<ElectionResponseDto>>.Ok(dtoList, "Elections retrieved successfully.");
    }

    public async Task<ApiResponse<ElectionResponseDto>> GetElectionByIdAsync(int id)
    {
        var election = await _context.Elections
            .Include(e => e.Positions.OrderBy(p => p.OrderIndex))
                .ThenInclude(p => p.Candidates)
            .Include(e => e.VoterParticipations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (election == null)
            return ApiResponse<ElectionResponseDto>.Fail($"Election with ID {id} was not found.");

        return ApiResponse<ElectionResponseDto>.Ok(MapToResponseDto(election), "Election retrieved successfully.");
    }

    public async Task<ApiResponse<ElectionResponseDto>> CreateElectionAsync(CreateElectionRequestDto request, int adminId)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<ElectionResponseDto>.Fail(
                "Invalid election parameters.",
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var election = new Election
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            AcademicYear = request.AcademicYear.Trim(),
            Scope = request.Scope,
            ScopeTarget = request.ScopeTarget.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = ElectionStatus.Draft,
            IsActive = true,
            AllowRealtimeResults = request.AllowRealtimeResults,
            CreatedAt = DateTime.UtcNow
        };

        _context.Elections.Add(election);
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.ElectionCreated,
            $"Created new election: '{election.Title}'",
            "Election",
            election.Id,
            request);

        return ApiResponse<ElectionResponseDto>.Ok(MapToResponseDto(election), "Election created successfully.");
    }

    public async Task<ApiResponse<ElectionResponseDto>> UpdateElectionAsync(int id, UpdateElectionRequestDto request, int adminId)
    {
        var election = await _context.Elections
            .Include(e => e.Positions)
                .ThenInclude(p => p.Candidates)
            .Include(e => e.VoterParticipations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (election == null)
            return ApiResponse<ElectionResponseDto>.Fail($"Election with ID {id} was not found.");

        if (request.EndDate <= request.StartDate)
            return ApiResponse<ElectionResponseDto>.Fail("End date must be after the start date.");

        election.Title = request.Title.Trim();
        election.Description = request.Description.Trim();
        if (!string.IsNullOrWhiteSpace(request.AcademicYear))
            election.AcademicYear = request.AcademicYear.Trim();
        election.Scope = request.Scope;
        election.ScopeTarget = request.ScopeTarget.Trim();
        election.StartDate = request.StartDate;
        election.EndDate = request.EndDate;
        election.AllowRealtimeResults = request.AllowRealtimeResults;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.ElectionCreated,
            $"Updated election #{id}: '{election.Title}'",
            "Election",
            election.Id,
            request);

        return ApiResponse<ElectionResponseDto>.Ok(MapToResponseDto(election), "Election updated successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateElectionStatusAsync(int id, ElectionStatus newStatus, int adminId)
    {
        var election = await _context.Elections.FindAsync(id);
        if (election == null)
            return ApiResponse<bool>.Fail($"Election with ID {id} was not found.");

        var oldStatus = election.Status;
        election.Status = newStatus;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.ElectionStatusChanged,
            $"Changed status of election #{id} from {oldStatus} to {newStatus}",
            "Election",
            election.Id,
            new { OldStatus = oldStatus, NewStatus = newStatus });

        return ApiResponse<bool>.Ok(true, $"Election status successfully changed to {newStatus}.");
    }

    public async Task<ApiResponse<bool>> DeleteElectionAsync(int id, int adminId)
    {
        var election = await _context.Elections
            .Include(e => e.VoterParticipations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (election == null)
            return ApiResponse<bool>.Fail($"Election with ID {id} was not found.");

        if (election.VoterParticipations.Count > 0)
            return ApiResponse<bool>.Fail("Cannot delete an election that has recorded voter participation. Archive or set status to Concluded instead.");

        _context.Elections.Remove(election);
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.ElectionStatusChanged,
            $"Deleted draft election #{id}: '{election.Title}'",
            "Election",
            id);

        return ApiResponse<bool>.Ok(true, "Election deleted successfully.");
    }

    private static ElectionResponseDto MapToResponseDto(Election election)
    {
        return new ElectionResponseDto
        {
            Id = election.Id,
            Title = election.Title,
            Description = election.Description,
            AcademicYear = election.AcademicYear,
            Scope = election.Scope,
            ScopeTarget = election.ScopeTarget,
            StartDate = election.StartDate,
            EndDate = election.EndDate,
            Status = election.Status,
            IsActive = election.IsActive,
            AllowRealtimeResults = election.AllowRealtimeResults,
            TotalPositions = election.Positions.Count,
            TotalVotersParticipated = election.VoterParticipations.Count,
            Positions = election.Positions.Select(p => new PositionResponseDto
            {
                Id = p.Id,
                ElectionId = p.ElectionId,
                Title = p.Title,
                Description = p.Description,
                MaxVotesAllowed = p.MaxVotesAllowed,
                OrderIndex = p.OrderIndex,
                Candidates = p.Candidates.Select(c => new CandidateResponseDto
                {
                    Id = c.Id,
                    PositionId = c.PositionId,
                    StudentId = c.StudentId,
                    FullName = c.FullName,
                    Nickname = c.Nickname,
                    Bio = c.Bio,
                    ManifestoUrl = c.ManifestoUrl,
                    PhotoUrl = c.PhotoUrl,
                    RunningMateName = c.RunningMateName,
                    RunningMatePhotoUrl = c.RunningMatePhotoUrl,
                    Status = c.Status
                }).ToList()
            }).ToList()
        };
    }
}
