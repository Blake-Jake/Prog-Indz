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
}
