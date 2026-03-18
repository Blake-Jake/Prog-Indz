using SQLite;

namespace EventMatch.Models;

public class GroupMember
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
}
