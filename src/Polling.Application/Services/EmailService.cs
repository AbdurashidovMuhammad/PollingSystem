using System.Net.Mail;
using System.Net;
using Polling.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Polling.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> SendVerificationEmailAsync(string email, string otpCode)
    {
        try
        {
            var subject = "Email Verification - Polling System";
            var body = $@"
                <html>
                <body>
                    <h2>Email Verification</h2>
                    <p>Your verification code is: <strong>{otpCode}</strong></p>
                    <p>This code will expire in 10 minutes.</p>
                    <p>If you didn't request this verification, please ignore this email.</p>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string otpCode)
    {
        try
        {
            var subject = "Password Reset - Polling System";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset</h2>
                    <p>Your password reset code is: <strong>{otpCode}</strong></p>
                    <p>This code will expire in 10 minutes.</p>
                    <p>If you didn't request a password reset, please ignore this email.</p>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string fullName)
    {
        try
        {
            var subject = "Welcome to Polling System";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome {fullName}!</h2>
                    <p>Thank you for registering with Polling System.</p>
                    <p>Your account has been created successfully.</p>
                    <p>Please verify your email address to complete the registration process.</p>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var smtpServer = smtpSettings["Server"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
            var smtpUsername = smtpSettings["Username"] ?? "";
            var smtpPassword = smtpSettings["Password"] ?? "";
            var fromEmail = smtpSettings["FromEmail"] ?? smtpUsername;

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            return true;
        }
        catch
        {
            return false;
        }
    }
}