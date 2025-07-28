using Polling.Application.DTOs;

namespace Polling.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SignUpAsync(SignUpRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
    Task<bool> ResendVerificationEmailAsync(ResendVerificationRequest request);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> RevokeTokenAsync(string refreshToken);
} 