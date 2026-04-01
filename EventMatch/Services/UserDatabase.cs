using SQLite;
using EventMatch.Models;

namespace EventMatch.Services;

public class UserDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public UserDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<User>().Wait();
        _database.CreateTableAsync<Profile>().Wait();
        _database.CreateTableAsync<Friend>().Wait(); // Add Friend table
        _database.CreateTableAsync<Group>().Wait();
        _database.CreateTableAsync<GroupMember>().Wait();
        _database.CreateTableAsync<GroupMessage>().Wait();
    }

    /// <summary>
    /// Get all users in local database
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _database.Table<User>().ToListAsync();
    }

    /// <summary>
    /// Get members for a specific group
    /// </summary>
    public async Task<List<GroupMember>> GetMembersForGroupAsync(int groupId)
    {
        return await _database.Table<GroupMember>().Where(m => m.GroupId == groupId).ToListAsync();
    }

    public Task<int> AddUserAsync(User user)
        => _database.InsertAsync(user);

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _database.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        return user;
    }

    // Group methods
    public async Task<List<Group>> GetGroupsAsync()
    {
        return await _database.Table<Group>().ToListAsync();
    }

    public async Task<Group?> GetGroupByCloudIdAsync(int cloudId)
    {
        if (cloudId == 0) return null;
        return await _database.Table<Group>().Where(g => g.CloudId == cloudId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get all groups where user is a member or owner
    /// </summary>
    public async Task<List<Group>> GetUserGroupsAsync(string userEmail)
    {
        System.Diagnostics.Debug.WriteLine($"[UserDatabase] GetUserGroupsAsync called for: {userEmail}");

        // Get all groups where user is owner
        var ownedGroups = await _database.Table<Group>().Where(g => g.OwnerEmail == userEmail).ToListAsync();
        System.Diagnostics.Debug.WriteLine($"[UserDatabase] Found {ownedGroups.Count} owned groups");
        foreach (var g in ownedGroups)
        {
            System.Diagnostics.Debug.WriteLine($"[UserDatabase]   - Owned: '{g.Name}' (ID: {g.Id}, Owner: {g.OwnerEmail})");
        }

        // Get all groups where user is a member
        var memberGroups = await _database.Table<GroupMember>().Where(m => m.UserEmail == userEmail).ToListAsync();
        System.Diagnostics.Debug.WriteLine($"[UserDatabase] Found {memberGroups.Count} group memberships");
        var memberGroupIds = memberGroups.Select(m => m.GroupId).ToHashSet();

        var result = new List<Group>();

        // Add owned groups
        foreach (var group in ownedGroups)
        {
            if (!result.Any(g => g.Id == group.Id))
                result.Add(group);
        }

        // Add member groups
        if (memberGroupIds.Count > 0)
        {
            var allGroups = await _database.Table<Group>().ToListAsync();

            foreach (var group in allGroups.Where(g => memberGroupIds.Contains(g.Id)))
            {
                if (!result.Any(g => g.Id == group.Id))
                    result.Add(group);
            }
        }

        return result;
    }

    public async Task<int> SaveGroupAsync(Group group)
    {
        // If this group came from cloud, prefer to dedupe by CloudId
        if (group.CloudId != 0)
        {
            var existing = await GetGroupByCloudIdAsync(group.CloudId);
            if (existing != null)
            {
                // Update existing local record with cloud data
                group.Id = existing.Id;
                System.Diagnostics.Debug.WriteLine($"[UserDatabase] Updating existing local group (by CloudId) ID: {group.Id}");
                return await _database.UpdateAsync(group);
            }
        }

        if (group.Id == 0)
        {
            // new group: insert and add owner as member
            var id = await _database.InsertAsync(group);
            System.Diagnostics.Debug.WriteLine($"[UserDatabase] Inserted group '{group.Name}', received ID: {id}");
            if (id > 0)
            {
                group.Id = (int)id;  // ensure ID is set on the group object
                System.Diagnostics.Debug.WriteLine($"[UserDatabase] Adding owner '{group.OwnerEmail}' as member to group {group.Id}");
                await _database.InsertAsync(new GroupMember { GroupId = group.Id, UserEmail = group.OwnerEmail });
            }
            return id;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[UserDatabase] Updating group ID: {group.Id}");
            return await _database.UpdateAsync(group);
        }
    }

    public async Task<bool> IsUserMemberOfGroupAsync(int groupId, string userEmail)
    {
        var gm = await _database.Table<GroupMember>().Where(g => g.GroupId == groupId && g.UserEmail == userEmail).FirstOrDefaultAsync();
        return gm != null;
    }

    public async Task AddUserToGroupAsync(int groupId, string userEmail)
    {
        var exists = await _database.Table<GroupMember>().Where(g => g.GroupId == groupId && g.UserEmail == userEmail).FirstOrDefaultAsync();
        if (exists == null)
        {
            await _database.InsertAsync(new GroupMember { GroupId = groupId, UserEmail = userEmail });
            var group = await _database.Table<Group>().Where(g => g.Id == groupId).FirstOrDefaultAsync();
            if (group != null)
            {
                group.MemberCount++;
                await _database.UpdateAsync(group);
            }
        }
    }

    public async Task<Group?> GetGroupByIdAsync(int id)
    {
        return await _database.Table<Group>().Where(g => g.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Group?> GetGroupByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return await _database.Table<Group>().Where(g => g.Name == name).FirstOrDefaultAsync();
    }

    public async Task DeleteGroupAsync(int groupId)
    {
        // delete messages
        var msgs = await _database.Table<GroupMessage>().Where(m => m.GroupId == groupId).ToListAsync();
        foreach (var m in msgs)
            await _database.DeleteAsync(m);

        // delete members
        var members = await _database.Table<GroupMember>().Where(g => g.GroupId == groupId).ToListAsync();
        foreach (var gm in members)
            await _database.DeleteAsync(gm);

        // delete group
        var group = await GetGroupByIdAsync(groupId);
        if (group != null)
            await _database.DeleteAsync(group);
    }

    /// <summary>
    /// Delete a group by its name (if exists). Removes messages, members and the group row.
    /// </summary>
    public async Task DeleteGroupByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        var group = await _database.Table<Group>().Where(g => g.Name == name).FirstOrDefaultAsync();
        if (group != null)
        {
            await DeleteGroupAsync(group.Id);
        }
    }

    /// <summary>
    /// Ensure the given user emails exist in the local database. If a user does not exist it will be created
    /// with the provided password. If it exists, the password will be updated to the provided value.
    /// </summary>
    public async Task SeedUsersAsync(IEnumerable<string> emails, string password = "asd")
    {
        if (emails == null) return;
        foreach (var email in emails)
        {
            if (string.IsNullOrWhiteSpace(email)) continue;
            var existing = await _database.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
            if (existing == null)
            {
                await _database.InsertAsync(new User { Email = email, Password = password });
            }
            else
            {
                existing.Password = password;
                await _database.UpdateAsync(existing);
            }
        }
    }

    public async Task<List<GroupMessage>> GetMessagesForGroupAsync(int groupId)
    {
        return await _database.Table<GroupMessage>().Where(m => m.GroupId == groupId).OrderBy(m => m.Timestamp).ToListAsync();
    }

    public async Task AddGroupMessageAsync(GroupMessage msg)
    {
        msg.Timestamp = DateTime.UtcNow;
        await _database.InsertAsync(msg);
    }

    public async Task<User?> GetUserAsync(string email, string password)
    {
        System.Diagnostics.Debug.WriteLine($"[UserDatabase] GetUserAsync called for email: {email}");
        try
        {
            var user = await _database.Table<User>().Where(u => u.Email == email && u.Password == password).FirstOrDefaultAsync();
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"[UserDatabase] User found: {user.Email}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[UserDatabase] User NOT found for email: {email}");
                // Debug: list all users in database
                var allUsers = await _database.Table<User>().ToListAsync();
                System.Diagnostics.Debug.WriteLine($"[UserDatabase] Total users in database: {allUsers.Count}");
                foreach (var u in allUsers)
                {
                    System.Diagnostics.Debug.WriteLine($"[UserDatabase]   - {u.Email}");
                }
            }
            return user;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserDatabase] Error in GetUserAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<int> UpdateUserAsync(User user)
        => await _database.UpdateAsync(user);

    // Profile methods
    public async Task<Profile?> GetProfileByEmailAsync(string email)
    {
        return await _database.Table<Profile>().Where(p => p.UserEmail == email).FirstOrDefaultAsync();
    }

    public async Task<int> SaveProfileAsync(Profile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.UserEmail))
            throw new ArgumentException("Profile must have UserEmail set.", nameof(profile));

        var existing = await GetProfileByEmailAsync(profile.UserEmail);
        if (existing == null)
        {
            return await _database.InsertAsync(profile);
        }
        else
        {
            // ensure we update the existing row
            profile.Id = existing.Id;
            return await _database.UpdateAsync(profile);
        }
    }

    // Friend methods
    public async Task<List<Profile>> GetFriendsAsync(string email)
    {
        // Only show friends where IsAccepted is true and current user is either sender or recipient
        var friends = await _database.Table<Friend>().Where(f => (f.UserEmail == email || f.FriendEmail == email) && f.IsAccepted).ToListAsync();
        var friendEmails = friends.Select(f => f.UserEmail == email ? f.FriendEmail : f.UserEmail).ToHashSet();
        return await _database.Table<Profile>().Where(p => friendEmails.Contains(p.UserEmail)).ToListAsync();
    }

    public async Task<List<Profile>> GetUnaddedUsersAsync(string email)
    {
        // Exclude users who are already friends or have pending requests (sent or received)
        var allProfiles = await _database.Table<Profile>().Where(p => p.UserEmail != email).ToListAsync();
        var friends = await _database.Table<Friend>().Where(f => (f.UserEmail == email || f.FriendEmail == email)).ToListAsync();
        var excludeEmails = friends.Select(f => f.UserEmail == email ? f.FriendEmail : f.UserEmail).ToHashSet();
        return allProfiles.Where(p => !excludeEmails.Contains(p.UserEmail)).ToList();
    }

    public async Task AddFriendAsync(string userEmail, string friendEmail)
    {
        var existing = await _database.Table<Friend>().Where(f => f.UserEmail == userEmail && f.FriendEmail == friendEmail).FirstOrDefaultAsync();
        if (existing == null)
        {
            // Set IsAccepted to false so it becomes a pending request
            await _database.InsertAsync(new Friend { UserEmail = userEmail, FriendEmail = friendEmail, IsAccepted = false });
        }
    }

    public async Task RemoveFriendAsync(string userEmail, string friendEmail)
    {
        var existing = await _database.Table<Friend>().Where(f => f.UserEmail == userEmail && f.FriendEmail == friendEmail).FirstOrDefaultAsync();
        if (existing != null)
        {
            await _database.DeleteAsync(existing);
        }
    }

    public async Task<List<Profile>> GetPendingRequestsAsync(string email)
    {
        // Requests where current user is the recipient and IsAccepted == false
        var requests = await _database.Table<Friend>().Where(f => f.FriendEmail == email && !f.IsAccepted).ToListAsync();
        var requesterEmails = requests.Select(f => f.UserEmail).ToHashSet();
        return await _database.Table<Profile>().Where(p => requesterEmails.Contains(p.UserEmail)).ToListAsync();
    }

    public async Task AcceptFriendAsync(string userEmail, string requesterEmail)
    {
        var request = await _database.Table<Friend>().Where(f => f.UserEmail == requesterEmail && f.FriendEmail == userEmail && !f.IsAccepted).FirstOrDefaultAsync();
        if (request != null)
        {
            request.IsAccepted = true;
            await _database.UpdateAsync(request);
            // Optionally, add reciprocal record
            var reciprocal = await _database.Table<Friend>().Where(f => f.UserEmail == userEmail && f.FriendEmail == requesterEmail).FirstOrDefaultAsync();
            if (reciprocal == null)
                await _database.InsertAsync(new Friend { UserEmail = userEmail, FriendEmail = requesterEmail, IsAccepted = true });
        }
    }

    public async Task DeclineFriendAsync(string userEmail, string requesterEmail)
    {
        var request = await _database.Table<Friend>().Where(f => f.UserEmail == requesterEmail && f.FriendEmail == userEmail && !f.IsAccepted).FirstOrDefaultAsync();
        if (request != null)
        {
            await _database.DeleteAsync(request);
        }
    }

    /// <summary>
    /// Delete all users from local database
    /// </summary>
    public async Task DeleteAllUsersAsync()
    {
        var users = await _database.Table<User>().ToListAsync();
        foreach (var user in users)
        {
            await _database.DeleteAsync(user);
        }
        System.Diagnostics.Debug.WriteLine($"[UserDatabase] Deleted {users.Count} users");
    }

    /// <summary>
    /// Delete all groups and related data (messages, members) from local database
    /// </summary>
    public async Task DeleteAllGroupsAsync()
    {
        var groups = await _database.Table<Group>().ToListAsync();
        foreach (var group in groups)
        {
            await DeleteGroupAsync(group.Id);
        }
        System.Diagnostics.Debug.WriteLine($"[UserDatabase] Deleted {groups.Count} groups");
    }

    /// <summary>
    /// Clear entire local database (all users, groups, profiles, friends)
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        System.Diagnostics.Debug.WriteLine("[UserDatabase] Clearing all local data...");

        // Delete all group-related data first
        var groups = await _database.Table<Group>().ToListAsync();
        foreach (var group in groups)
        {
            await DeleteGroupAsync(group.Id);
        }

        // Delete all messages
        var messages = await _database.Table<GroupMessage>().ToListAsync();
        foreach (var msg in messages)
        {
            await _database.DeleteAsync(msg);
        }

        // Delete all friends
        var friends = await _database.Table<Friend>().ToListAsync();
        foreach (var friend in friends)
        {
            await _database.DeleteAsync(friend);
        }

        // Delete all profiles
        var profiles = await _database.Table<Profile>().ToListAsync();
        foreach (var profile in profiles)
        {
            await _database.DeleteAsync(profile);
        }

        // Delete all users last
        var users = await _database.Table<User>().ToListAsync();
        foreach (var user in users)
        {
            await _database.DeleteAsync(user);
        }

        System.Diagnostics.Debug.WriteLine("[UserDatabase] All local data cleared successfully");
    }
}
