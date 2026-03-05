using SQLite;

namespace EventMatch.Models;

public class Profile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Link to user account (email used here to keep it simple)
    public string UserEmail { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public int RadiusKm { get; set; } = 10;
    public string Description { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
}