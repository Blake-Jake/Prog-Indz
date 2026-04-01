// Utility to check local database contents in EventMatch MAUI app
// Add this to your app to view what data exists locally

using EventMatch.Models;
using EventMatch.Services;
using System.Diagnostics;

namespace EventMatch.Utils;

/// <summary>
/// Debug utility to display local database contents
/// Use this to check what data is stored on the device
/// </summary>
public class DataStatusUtil
{
    private readonly UserDatabase _database;

    public DataStatusUtil(UserDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// Print all local data to debug output
    /// </summary>
    public async Task PrintAllDataAsync()
    {
        Debug.WriteLine("\n" + new string('=', 60));
        Debug.WriteLine("[LOCAL DATABASE STATUS]");
        Debug.WriteLine(new string('=', 60));

        try
        {
            // Users
            await PrintUsersAsync();
            
            // Groups
            await PrintGroupsAsync();
            
            // Profiles
            await PrintProfilesAsync();
            
            // Friends
            await PrintFriendsAsync();
            
            // Group Members
            await PrintGroupMembersAsync();
            
            // Group Messages
            await PrintGroupMessagesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] Failed to read database: {ex.Message}");
        }

        Debug.WriteLine(new string('=', 60));
        Debug.WriteLine("[END STATUS]");
        Debug.WriteLine(new string('=', 60) + "\n");
    }

    private async Task PrintUsersAsync()
    {
        try
        {
            var users = await _database.GetAllUsersAsync();
            Debug.WriteLine($"\n[USERS] Total: {users.Count}");
            if (users.Count == 0)
            {
                Debug.WriteLine("  (no users found)");
            }
            else
            {
                foreach (var user in users)
                {
                    Debug.WriteLine($"  - Email: {user.Email}");
                    Debug.WriteLine($"    ID: {user.Id}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"  [ERROR] {ex.Message}");
        }
    }

    private async Task PrintGroupsAsync()
    {
        try
        {
            var groups = await _database.GetGroupsAsync();
            Debug.WriteLine($"\n[GROUPS] Total: {groups.Count}");
            if (groups.Count == 0)
            {
                Debug.WriteLine("  (no groups found)");
            }
            else
            {
                foreach (var group in groups)
                {
                    Debug.WriteLine($"  - Group: {group.Name}");
                    Debug.WriteLine($"    ID: {group.Id}, Owner: {group.OwnerEmail}");
                    Debug.WriteLine($"    Members: {group.MemberCount}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"  [ERROR] {ex.Message}");
        }
    }

    private async Task PrintProfilesAsync()
    {
        try
        {
            var users = await _database.GetAllUsersAsync();
            var profiles = new List<Profile>();
            
            foreach (var user in users)
            {
                var profile = await _database.GetProfileByEmailAsync(user.Email);
                if (profile != null)
                    profiles.Add(profile);
            }

            Debug.WriteLine($"\n[PROFILES] Total: {profiles.Count}");
            if (profiles.Count == 0)
            {
                Debug.WriteLine("  (no profiles found)");
            }
            else
            {
                foreach (var profile in profiles)
                {
                    Debug.WriteLine($"  - Email: {profile.UserEmail}");
                    Debug.WriteLine($"    Username: {profile.Username}, Tag: {profile.Tag}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"  [ERROR] {ex.Message}");
        }
    }

    private async Task PrintFriendsAsync()
    {
        try
        {
            var users = await _database.GetAllUsersAsync();
            var totalFriends = 0;

            Debug.WriteLine($"\n[FRIENDS]");
            foreach (var user in users)
            {
                var friends = await _database.GetFriendsAsync(user.Email);
                totalFriends += friends.Count;
                
                if (friends.Count > 0)
                {
                    Debug.WriteLine($"  User: {user.Email} has {friends.Count} friends:");
                    foreach (var friend in friends)
                    {
                        Debug.WriteLine($"    - {friend.UserEmail} ({friend.Username})");
                    }
                }
            }
            Debug.WriteLine($"  Total friendships: {totalFriends}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"  [ERROR] {ex.Message}");
        }
    }

    private async Task PrintGroupMembersAsync()
    {
        try
        {
            var groups = await _database.GetGroupsAsync();
            Debug.WriteLine($"\n[GROUP MEMBERS]");
            
            if (groups.Count == 0)
            {
                Debug.WriteLine("  (no groups)");
            }
            else
            {
                foreach (var group in groups)
                {
                    var members = await _database.GetMembersForGroupAsync(group.Id);
                    Debug.WriteLine($"  Group: {group.Name} ({group.Id})");
                    Debug.WriteLine($"    Members: {members.Count}");
                    foreach (var member in members)
                    {
                        Debug.WriteLine($"      - {member.UserEmail}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"  [ERROR] {ex.Message}");
        }
    }

    private async Task PrintGroupMessagesAsync()
    {
        try
        {
            var groups = await _database.GetGroupsAsync();
            var totalMessages = 0;

            Debug.WriteLine($"\n[GROUP MESSAGES]");
            
            if (groups.Count == 0)
            {
                Debug.WriteLine("  (no groups)");
            }
            else
            {
                foreach (var group in groups)
                {
                    var messages = await _database.GetMessagesForGroupAsync(group.Id);
                    totalMessages += messages.Count;
                    
                    if (messages.Count > 0)
                    {
                        Debug.WriteLine($"  Group: {group.Name} ({group.Id})");
                        Debug.WriteLine($"    Messages: {messages.Count}");
                        foreach (var msg in messages.TakeLast(3))
                        {
                            Debug.WriteLine($"      - {msg.FromEmail}: {msg.Text} ({msg.Timestamp})");
                        }
                    }
                }
            }
            Debug.WriteLine($"  Total messages: {totalMessages}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"  [ERROR] {ex.Message}");
        }
    }

    /// <summary>
    /// Get a summary as string (for UI display)
    /// </summary>
    public async Task<string> GetSummaryAsync()
    {
        var summary = new System.Text.StringBuilder();
        
        try
        {
            var users = await _database.GetAllUsersAsync();
            var groups = await _database.GetGroupsAsync();
            
            summary.AppendLine("=== LOCAL DATABASE ===");
            summary.AppendLine($"Users: {users.Count}");
            summary.AppendLine($"Groups: {groups.Count}");
            
            if (users.Count > 0)
            {
                summary.AppendLine("\nUsers:");
                foreach (var user in users)
                {
                    summary.AppendLine($"  - {user.Email}");
                }
            }
            
            if (groups.Count > 0)
            {
                summary.AppendLine("\nGroups:");
                foreach (var group in groups)
                {
                    summary.AppendLine($"  - {group.Name} ({group.MemberCount} members)");
                }
            }
        }
        catch (Exception ex)
        {
            summary.AppendLine($"Error: {ex.Message}");
        }

        return summary.ToString();
    }
}
