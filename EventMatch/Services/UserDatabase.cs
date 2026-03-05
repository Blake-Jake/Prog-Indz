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
}
