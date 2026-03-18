using SQLite;

namespace EventMatch.Models;

public class GroupMessage
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
