namespace Polling.Core.Entities;

public class Option
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public string Text { get; set; } = null!;
    public int Votes { get; set; } = 0;
    public Poll Poll { get; set; } = null!;
}
