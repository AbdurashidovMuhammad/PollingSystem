using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polling.Application.DTOs;
using Polling.Core.Entities;
using Polling.DataAccess.Persistence;

namespace Polling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
            return Unauthorized(new { message = "User not authenticated" });

        // Get fresh user data from database
        var freshUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (freshUser == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserDto
        {
            Id = freshUser.Id,
            FullName = freshUser.FullName,
            Email = freshUser.Email,
            Role = freshUser.Role.ToString(),
            IsEmailVerified = freshUser.IsEmailVerified
        });
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
            return Unauthorized(new { message = "User not authenticated" });

        // Get fresh user data from database
        var freshUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (freshUser == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserDto
        {
            Id = freshUser.Id,
            FullName = freshUser.FullName,
            Email = freshUser.Email,
            Role = freshUser.Role.ToString(),
            IsEmailVerified = freshUser.IsEmailVerified
        });
    }

    [HttpGet("all")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsEmailVerified = u.IsEmailVerified
            })
            .ToListAsync();

        return Ok(users);
    }
}