﻿namespace Polling.Core.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime? ExpiryDate { get; set; }
    public bool IsRevoked { get; set; } = false;
    public User User { get; set; } = null!;
}
