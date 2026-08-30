using EVoteUG.Core.DTOs.Results;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Core.Interfaces;

public interface IResultsService
{
    Task<ApiResponse<ElectionResultsResponseDto>> GetElectionResultsAsync(int electionId, bool isPrivilegedCaller = false);
    Task<ApiResponse<TurnoutAnalyticsDto>> GetTurnoutAnalyticsAsync(int electionId);
}
