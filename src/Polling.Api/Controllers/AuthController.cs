using Microsoft.AspNetCore.Mvc;
using Polling.Application.DTOs;
using Polling.Application.Interfaces;

namespace Polling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignUpRequest request)
    {
        try
        {
            var response = await _authService.SignUpAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(request);
            if (result)
                return Ok(new { message = "Email verified successfully" });
            
            return BadRequest(new { message = "Invalid verification code" });
        }
        catch
        {
            return StatusCode(500, new { message = "An error occurred during email verification" });
        }
    }

    [HttpPost("resend-verification")]
    public async Task<ActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        try
        {
            var result = await _authService.ResendVerificationEmailAsync(request);
            if (result)
                return Ok(new { message = "Verification email sent successfully" });
            
            return BadRequest(new { message = "Email not found" });
        }
        catch
        {
            return StatusCode(500, new { message = "An error occurred while sending verification email" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var result = await _authService.ForgotPasswordAsync(request);
            if (result)
                return Ok(new { message = "Password reset email sent successfully" });
            
            return BadRequest(new { message = "Email not found" });
        }
        catch
        {
            return StatusCode(500, new { message = "An error occurred while sending password reset email" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (result)
                return Ok(new { message = "Password reset successfully" });
            
            return BadRequest(new { message = "Invalid reset code or passwords do not match" });
        }
        catch
        {
            return StatusCode(500, new { message = "An error occurred during password reset" });
        }
    }

    [HttpPost("revoke-token")]
    public async Task<ActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RevokeTokenAsync(request.RefreshToken);
            if (result)
                return Ok(new { message = "Token revoked successfully" });
            
            return BadRequest(new { message = "Invalid refresh token" });
        }
        catch
        {
            return StatusCode(500, new { message = "An error occurred while revoking token" });
        }
    }
} 