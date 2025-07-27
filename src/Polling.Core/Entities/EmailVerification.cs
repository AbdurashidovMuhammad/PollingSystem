namespace Polling.Core.Entities;

public class EmailVerification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OtpCode { get; set; } = null!;
    public DateTime ExpiryTime { get; set; }
    public bool IsUsed { get; set; } = false;
    public User User { get; set; } = null!;
}
