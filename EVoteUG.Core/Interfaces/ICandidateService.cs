using EVoteUG.Core.DTOs.Candidate;
using EVoteUG.Shared.Enums;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Core.Interfaces;

public interface ICandidateService
{
    Task<ApiResponse<List<CandidateResponseDto>>> GetCandidatesByPositionAsync(int positionId);
    Task<ApiResponse<CandidateResponseDto>> GetCandidateByIdAsync(int id);
    Task<ApiResponse<CandidateResponseDto>> CreateCandidateAsync(CreateCandidateRequestDto request, int adminId);
    Task<ApiResponse<CandidateResponseDto>> UpdateCandidateAsync(int id, UpdateCandidateRequestDto request, int adminId);
    Task<ApiResponse<bool>> UpdateCandidateStatusAsync(int id, CandidateStatus status, int adminId);
    Task<ApiResponse<string>> UploadCandidatePhotoAsync(int id, Stream fileStream, string fileName, int adminId);
    Task<ApiResponse<string>> UploadCandidateManifestoAsync(int id, Stream fileStream, string fileName, int adminId);
}
