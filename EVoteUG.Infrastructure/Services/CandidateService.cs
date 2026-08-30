using EVoteUG.Core.DTOs.Candidate;
using EVoteUG.Core.Interfaces;
using EVoteUG.Core.Validators;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Infrastructure.Storage;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Infrastructure.Services;

public class CandidateService : ICandidateService
{
    private readonly EVoteUGDbContext _context;
    private readonly IAuditService _auditService;
    private readonly LocalFileStorageService _storageService;
    private readonly CreateCandidateValidator _createValidator;

    public CandidateService(
        EVoteUGDbContext context, 
        IAuditService auditService, 
        LocalFileStorageService storageService)
    {
        _context = context;
        _auditService = auditService;
        _storageService = storageService;
        _createValidator = new CreateCandidateValidator();
    }

    public async Task<ApiResponse<List<CandidateResponseDto>>> GetCandidatesByPositionAsync(int positionId)
    {
        var candidates = await _context.Candidates
            .Where(c => c.PositionId == positionId)
            .AsNoTracking()
            .ToListAsync();

        var dtos = candidates.Select(MapToDto).ToList();
        return ApiResponse<List<CandidateResponseDto>>.Ok(dtos, "Candidates retrieved successfully.");
    }

    public async Task<ApiResponse<CandidateResponseDto>> GetCandidateByIdAsync(int id)
    {
        var candidate = await _context.Candidates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (candidate == null)
            return ApiResponse<CandidateResponseDto>.Fail($"Candidate with ID {id} was not found.");

        return ApiResponse<CandidateResponseDto>.Ok(MapToDto(candidate), "Candidate retrieved successfully.");
    }

    public async Task<ApiResponse<CandidateResponseDto>> CreateCandidateAsync(CreateCandidateRequestDto request, int adminId)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<CandidateResponseDto>.Fail(
                "Invalid candidate parameters.",
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var positionExists = await _context.Positions.AnyAsync(p => p.Id == request.PositionId);
        if (!positionExists)
            return ApiResponse<CandidateResponseDto>.Fail($"Position with ID {request.PositionId} does not exist.");

        var candidate = new Candidate
        {
            PositionId = request.PositionId,
            StudentId = request.StudentId.Trim(),
            FullName = request.FullName.Trim(),
            Nickname = request.Nickname.Trim(),
            Bio = request.Bio.Trim(),
            RunningMateName = request.RunningMateName.Trim(),
            Status = CandidateStatus.Approved
        };

        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.CandidateRegistered,
            $"Registered candidate '{candidate.FullName}' for Position #{candidate.PositionId}",
            "Candidate",
            candidate.Id,
            request);

        return ApiResponse<CandidateResponseDto>.Ok(MapToDto(candidate), "Candidate registered successfully.");
    }

    public async Task<ApiResponse<CandidateResponseDto>> UpdateCandidateAsync(int id, UpdateCandidateRequestDto request, int adminId)
    {
        var candidate = await _context.Candidates.FindAsync(id);
        if (candidate == null)
            return ApiResponse<CandidateResponseDto>.Fail($"Candidate with ID {id} was not found.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return ApiResponse<CandidateResponseDto>.Fail("Candidate full name is required.");

        candidate.FullName = request.FullName.Trim();
        candidate.Nickname = request.Nickname.Trim();
        candidate.Bio = request.Bio.Trim();
        candidate.RunningMateName = request.RunningMateName.Trim();

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.CandidateRegistered,
            $"Updated candidate profile #{id}: '{candidate.FullName}'",
            "Candidate",
            candidate.Id,
            request);

        return ApiResponse<CandidateResponseDto>.Ok(MapToDto(candidate), "Candidate profile updated successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateCandidateStatusAsync(int id, CandidateStatus status, int adminId)
    {
        var candidate = await _context.Candidates.FindAsync(id);
        if (candidate == null)
            return ApiResponse<bool>.Fail($"Candidate with ID {id} was not found.");

        var oldStatus = candidate.Status;
        candidate.Status = status;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.CandidateStatusChanged,
            $"Changed status of candidate #{id} ({candidate.FullName}) from {oldStatus} to {status}",
            "Candidate",
            candidate.Id,
            new { OldStatus = oldStatus, NewStatus = status });

        return ApiResponse<bool>.Ok(true, $"Candidate status updated to {status}.");
    }

    public async Task<ApiResponse<string>> UploadCandidatePhotoAsync(int id, Stream fileStream, string fileName, int adminId)
    {
        var candidate = await _context.Candidates.FindAsync(id);
        if (candidate == null)
            return ApiResponse<string>.Fail($"Candidate with ID {id} was not found.");

        try
        {
            if (!string.IsNullOrWhiteSpace(candidate.PhotoUrl))
            {
                _storageService.DeleteFile(candidate.PhotoUrl);
            }

            var relativeUrl = await _storageService.SaveFileAsync(fileStream, fileName, "candidates");
            candidate.PhotoUrl = relativeUrl;
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                adminId,
                AuditEventType.CandidateRegistered,
                $"Uploaded portrait photo for candidate #{id} ({candidate.FullName})",
                "Candidate",
                candidate.Id);

            return ApiResponse<string>.Ok(relativeUrl, "Candidate photo uploaded successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"File upload failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> UploadCandidateManifestoAsync(int id, Stream fileStream, string fileName, int adminId)
    {
        var candidate = await _context.Candidates.FindAsync(id);
        if (candidate == null)
            return ApiResponse<string>.Fail($"Candidate with ID {id} was not found.");

        try
        {
            if (!string.IsNullOrWhiteSpace(candidate.ManifestoUrl))
            {
                _storageService.DeleteFile(candidate.ManifestoUrl);
            }

            var relativeUrl = await _storageService.SaveFileAsync(fileStream, fileName, "manifestos");
            candidate.ManifestoUrl = relativeUrl;
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                adminId,
                AuditEventType.CandidateRegistered,
                $"Uploaded campaign manifesto PDF for candidate #{id} ({candidate.FullName})",
                "Candidate",
                candidate.Id);

            return ApiResponse<string>.Ok(relativeUrl, "Candidate manifesto PDF uploaded successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"Manifesto upload failed: {ex.Message}");
        }
    }

    private static CandidateResponseDto MapToDto(Candidate c)
    {
        return new CandidateResponseDto
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
        };
    }
}
