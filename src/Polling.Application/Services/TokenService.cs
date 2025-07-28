using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Polling.Application.Interfaces;
using Polling.Core.Entities;
using Polling.DataAccess.Persistence;

namespace Polling.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public TokenService(IConfiguration configuration, AppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "your-super-secret-key-with-at-least-32-characters");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("IsEmailVerified", user.IsEmailVerified.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"] ?? "PollingSystem",
            Audience = _configuration["Jwt:Audience"] ?? "PollingSystem",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        var refreshTokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken &&
                                     !rt.IsRevoked &&
                                     rt.ExpiryDate > DateTime.UtcNow);

        return refreshTokenEntity != null;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        var refreshTokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (refreshTokenEntity == null)
            return false;

        refreshTokenEntity.IsRevoked = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetUserFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "your-super-secret-key-with-at-least-32-characters");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "PollingSystem",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "PollingSystem",
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
        catch
        {
            return null;
        }
    }
}