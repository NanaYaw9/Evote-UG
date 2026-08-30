using EVoteUG.Core.DTOs.Admin;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Core.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync();
    Task<ApiResponse<VoterImportResultDto>> ImportVoterRollCsvAsync(Stream csvStream, int adminId);
}
