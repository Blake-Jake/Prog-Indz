using SQLite;

namespace EventMatch.Models;

public class Friend
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty; // who sent the request
    public string FriendEmail { get; set; } = string.Empty; // who is being added
    public bool IsAccepted { get; set; } = false; // true if friendship is mutual
}
