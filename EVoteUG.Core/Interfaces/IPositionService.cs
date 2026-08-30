using EVoteUG.Core.DTOs.Position;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Core.Interfaces;

public interface IPositionService
{
    Task<ApiResponse<List<PositionResponseDto>>> GetPositionsByElectionAsync(int electionId);
    Task<ApiResponse<PositionResponseDto>> GetPositionByIdAsync(int id);
    Task<ApiResponse<PositionResponseDto>> CreatePositionAsync(CreatePositionRequestDto request, int adminId);
    Task<ApiResponse<PositionResponseDto>> UpdatePositionAsync(int id, UpdatePositionRequestDto request, int adminId);
    Task<ApiResponse<bool>> DeletePositionAsync(int id, int adminId);
}
