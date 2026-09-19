using EVoteUG.Core.DTOs.Election;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Core.Interfaces;

public interface IElectionService
{
    Task<ApiResponse<List<ElectionResponseDto>>> GetElectionsAsync(ElectionStatus? status = null, ElectionScope? scope = null);
    Task<ApiResponse<ElectionResponseDto>> GetElectionByIdAsync(int id);
    Task<ApiResponse<ElectionResponseDto>> CreateElectionAsync(CreateElectionRequestDto request, int adminId);
    Task<ApiResponse<ElectionResponseDto>> UpdateElectionAsync(int id, UpdateElectionRequestDto request, int adminId);
    Task<ApiResponse<bool>> UpdateElectionStatusAsync(int id, ElectionStatus newStatus, int adminId);
    Task<ApiResponse<bool>> DeleteElectionAsync(int id, int adminId);
}
