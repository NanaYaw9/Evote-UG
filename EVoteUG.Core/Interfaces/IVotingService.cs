using EVoteUG.Core.DTOs.Voting;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Core.Interfaces;

public interface IVotingService
{
    Task<ApiResponse<BallotResponseDto>> GetEligibleBallotAsync(int electionId, int studentId);
    Task<ApiResponse<VoterStatusResponseDto>> CheckVoterStatusAsync(int electionId, int studentId);
    Task<ApiResponse<VoteReceiptResponseDto>> CastBallotAsync(int studentId, CastBallotRequestDto request, string ipAddress, string deviceInfo = "");
    Task<ApiResponse<List<VoteReceiptResponseDto>>> GetStudentReceiptsAsync(int studentId);
    Task<ApiResponse<bool>> VerifyReceiptHashAsync(string receiptHash);
}
