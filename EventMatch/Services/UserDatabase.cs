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
    }

    public Task<int> AddUserAsync(User user)
        => _database.InsertAsync(user);

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _database.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        return user;
    }

    public async Task<User?> GetUserAsync(string email, string password)
    {
        var user = await _database.Table<User>().Where(u => u.Email == email && u.Password == password).FirstOrDefaultAsync();
        return user;
    }

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
}
