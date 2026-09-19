using EVoteUG.Core.DTOs.Candidate;
using EVoteUG.Core.DTOs.Position;
using EVoteUG.Core.Interfaces;
using EVoteUG.Core.Validators;
using EVoteUG.Infrastructure.Data;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Models;
using EVoteUG.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace EVoteUG.Infrastructure.Services;

public class PositionService : IPositionService
{
    private readonly EVoteUGDbContext _context;
    private readonly IAuditService _auditService;
    private readonly CreatePositionValidator _createValidator;

    public PositionService(EVoteUGDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
        _createValidator = new CreatePositionValidator();
    }

    public async Task<ApiResponse<List<PositionResponseDto>>> GetPositionsByElectionAsync(int electionId)
    {
        var positions = await _context.Positions
            .Include(p => p.Candidates)
            .Where(p => p.ElectionId == electionId)
            .OrderBy(p => p.OrderIndex)
            .AsNoTracking()
            .ToListAsync();

        var dtos = positions.Select(MapToDto).ToList();
        return ApiResponse<List<PositionResponseDto>>.Ok(dtos, "Positions retrieved successfully.");
    }

    public async Task<ApiResponse<PositionResponseDto>> GetPositionByIdAsync(int id)
    {
        var position = await _context.Positions
            .Include(p => p.Candidates)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position == null)
            return ApiResponse<PositionResponseDto>.Fail($"Position with ID {id} was not found.");

        return ApiResponse<PositionResponseDto>.Ok(MapToDto(position), "Position retrieved successfully.");
    }

    public async Task<ApiResponse<PositionResponseDto>> CreatePositionAsync(CreatePositionRequestDto request, int adminId)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<PositionResponseDto>.Fail(
                "Invalid position parameters.",
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var electionExists = await _context.Elections.AnyAsync(e => e.Id == request.ElectionId);
        if (!electionExists)
            return ApiResponse<PositionResponseDto>.Fail($"Election with ID {request.ElectionId} does not exist.");

        var position = new Position
        {
            ElectionId = request.ElectionId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            MaxVotesAllowed = request.MaxVotesAllowed,
            OrderIndex = request.OrderIndex
        };

        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.ElectionCreated,
            $"Created position '{position.Title}' for Election #{position.ElectionId}",
            "Position",
            position.Id,
            request);

        return ApiResponse<PositionResponseDto>.Ok(MapToDto(position), "Position created successfully.");
    }

    public async Task<ApiResponse<PositionResponseDto>> UpdatePositionAsync(int id, UpdatePositionRequestDto request, int adminId)
    {
        var position = await _context.Positions
            .Include(p => p.Candidates)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position == null)
            return ApiResponse<PositionResponseDto>.Fail($"Position with ID {id} was not found.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return ApiResponse<PositionResponseDto>.Fail("Position title cannot be empty.");

        position.Title = request.Title.Trim();
        position.Description = request.Description.Trim();
        position.MaxVotesAllowed = request.MaxVotesAllowed < 1 ? 1 : request.MaxVotesAllowed;
        position.OrderIndex = request.OrderIndex;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.ElectionCreated,
            $"Updated position #{id}: '{position.Title}'",
            "Position",
            position.Id,
            request);

        return ApiResponse<PositionResponseDto>.Ok(MapToDto(position), "Position updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeletePositionAsync(int id, int adminId)
    {
        var position = await _context.Positions
            .Include(p => p.Candidates)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position == null)
            return ApiResponse<bool>.Fail($"Position with ID {id} was not found.");

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            adminId,
            AuditEventType.ElectionCreated,
            $"Deleted position #{id}: '{position.Title}'",
            "Position",
            id);

        return ApiResponse<bool>.Ok(true, "Position deleted successfully.");
    }

    private static PositionResponseDto MapToDto(Position p)
    {
        return new PositionResponseDto
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
        };
    }
}
