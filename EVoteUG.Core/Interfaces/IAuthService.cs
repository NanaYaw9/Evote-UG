using EVoteUG.Core.DTOs.Auth;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Core.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthTokenResponseDto>> StudentLoginAsync(StudentLoginRequestDto request);
    Task<ApiResponse<StudentResponseDto>> RegisterStudentAsync(StudentRegisterRequestDto request);
    Task<ApiResponse<StudentResponseDto>> StudentDirectLoginAsync(string identifier, string password);
    Task<ApiResponse<AuthTokenResponseDto>> AdminLoginAsync(AdminLoginRequestDto request);
    Task<ApiResponse<UserProfileResponseDto>> GetUserProfileAsync(int userId, string role);
    Task<ApiResponse<bool>> ChangePasswordAsync(int userId, string role, ChangePasswordRequestDto request);
}
