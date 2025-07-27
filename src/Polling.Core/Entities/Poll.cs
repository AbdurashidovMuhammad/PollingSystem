namespace Polling.Core.Entities;

public class Poll
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Option> Options { get; set; } = null!;
}
