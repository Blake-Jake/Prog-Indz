using SQLite;

namespace EventMatch.Models;

public class Group
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MemberCount { get; set; } = 0;
    // Owner of the group (email)
    public string OwnerEmail { get; set; } = string.Empty;

    // Runtime-only flags - do not persist
    [Ignore]
    public bool IsOwner { get; set; } = false;

    [Ignore]
    public bool IsMember { get; set; } = false;

    [Ignore]
    public bool CanJoin => !IsOwner && !IsMember;
    public bool IsPublic { get; set; } = true;
}
