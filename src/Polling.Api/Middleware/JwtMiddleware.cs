using Microsoft.AspNetCore.Http;
using Polling.Application.Interfaces;
using Polling.Core.Entities;

namespace Polling.Api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            try
            {
                var user = await tokenService.GetUserFromTokenAsync(token);
                if (user != null)
                {
                    context.Items["User"] = user;
                }
            }
            catch
            {
                // Token is invalid, but we don't want to block the request
                // The controller can handle authentication as needed
            }
        }

        await _next(context);
    }
} 