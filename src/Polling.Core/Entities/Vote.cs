namespace Polling.Core.Entities;

public class Vote
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid OptionId { get; set; }
    public Guid UserId { get; set; }
    public Poll Poll { get; set; } = null!;
    public Option Option { get; set; } = null!;
    public User User { get; set; } = null!;
}
