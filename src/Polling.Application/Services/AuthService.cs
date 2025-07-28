using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Polling.Application.DTOs;
using Polling.Application.Interfaces;
using Polling.Core.Entities;
using Polling.Core.Enums;
using Polling.DataAccess.Persistence;

namespace Polling.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext context, ITokenService tokenService, IEmailService emailService)
    {
        _context = context;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<AuthResponse> SignUpAsync(SignUpRequest request)
    {
        // Validate request
        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match");

        // Check if email exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (existingUser != null)
            throw new ArgumentException("Email already exists");

        // Create password hash and salt
        var (passwordHash, passwordSalt) = HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = UserRole.User,
            IsEmailVerified = false
        };

        _context.Users.Add(user);

        // Generate OTP for email verification
        var otpCode = GenerateOtpCode();
        var emailVerification = new EmailVerification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            OtpCode = otpCode,
            ExpiryTime = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        _context.EmailVerifications.Add(emailVerification);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        // Send verification email
        await _emailService.SendVerificationEmailAsync(user.Email, otpCode);
        await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (user == null)
            throw new ArgumentException("Invalid email or password");

        if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            throw new ArgumentException("Invalid email or password");

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshTokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken &&
                                     !rt.IsRevoked &&
                                     rt.ExpiryDate > DateTime.UtcNow);

        if (refreshTokenEntity == null)
            throw new ArgumentException("Invalid refresh token");

        var user = refreshTokenEntity.User;
        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Revoke old refresh token
        refreshTokenEntity.IsRevoked = true;

        // Save new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified
            }
        };
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (user == null)
            return false;

        var emailVerification = await _context.EmailVerifications
            .FirstOrDefaultAsync(ev => ev.UserId == user.Id &&
                                     ev.OtpCode == request.OtpCode &&
                                     !ev.IsUsed &&
                                     ev.ExpiryTime > DateTime.UtcNow);

        if (emailVerification == null)
            return false;

        user.IsEmailVerified = true;
        emailVerification.IsUsed = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResendVerificationEmailAsync(ResendVerificationRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (user == null)
            return false;

        var otpCode = GenerateOtpCode();
        var emailVerification = new EmailVerification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            OtpCode = otpCode,
            ExpiryTime = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        _context.EmailVerifications.Add(emailVerification);
        await _context.SaveChangesAsync();

        return await _emailService.SendVerificationEmailAsync(user.Email, otpCode);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (user == null)
            return false;

        var otpCode = GenerateOtpCode();
        var emailVerification = new EmailVerification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            OtpCode = otpCode,
            ExpiryTime = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        _context.EmailVerifications.Add(emailVerification);
        await _context.SaveChangesAsync();

        return await _emailService.SendPasswordResetEmailAsync(user.Email, otpCode);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return false;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (user == null)
            return false;

        var emailVerification = await _context.EmailVerifications
            .FirstOrDefaultAsync(ev => ev.UserId == user.Id &&
                                     ev.OtpCode == request.OtpCode &&
                                     !ev.IsUsed &&
                                     ev.ExpiryTime > DateTime.UtcNow);

        if (emailVerification == null)
            return false;

        var (passwordHash, passwordSalt) = HashPassword(request.NewPassword);
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        emailVerification.IsUsed = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var refreshTokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (refreshTokenEntity == null)
            return false;

        refreshTokenEntity.IsRevoked = true;
        await _context.SaveChangesAsync();
        return true;
    }

    private static (string hash, string salt) HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hash = hmac.ComputeHash(passwordBytes);
        var salt = hmac.Key;

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(storedSalt) || string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        try
        {
            using var algorithm = new Rfc2898DeriveBytes(
                password: password,
                salt: Encoding.UTF8.GetBytes(storedSalt),
                iterations: 1000,
                hashAlgorithm: HashAlgorithmName.SHA256);

            var computedHashBytes = algorithm.GetBytes(32);

            var computedHash = Convert.ToBase64String(computedHashBytes);

            return computedHash == storedHash;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parol tekshirishda xato yuz berdi: {ex.Message}");
            return false;
        }
    }

    private static string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}