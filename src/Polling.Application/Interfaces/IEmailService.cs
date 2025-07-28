namespace Polling.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(string email, string otpCode);
    Task<bool> SendPasswordResetEmailAsync(string email, string otpCode);
    Task<bool> SendWelcomeEmailAsync(string email, string fullName);
} 