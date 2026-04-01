using EventMatch.Models;
using System.Diagnostics;

namespace EventMatch.Services;

/// <summary>
/// Hybrid authentication service that syncs between cloud and local database
/// Uses cloud storage as primary, with local SQLite as fallback/cache
/// </summary>
public class HybridAuthService
{
    private readonly CloudAuthService _cloudAuthService;
    private readonly UserDatabase _localDatabase;
    private bool _isCloudAvailable = false;

    public HybridAuthService(CloudAuthService cloudAuthService, UserDatabase localDatabase)
    {
        _cloudAuthService = cloudAuthService;
        _localDatabase = localDatabase;
    }

    /// <summary>
    /// Initialize cloud connectivity check
    /// </summary>
    public async Task InitializeAsync()
    {
        // Check actual cloud connectivity instead of forcing offline mode
        try
        {
            _isCloudAvailable = await CheckCloudConnectivityAsync();
            Debug.WriteLine(_isCloudAvailable
                ? "[HybridAuthService] Cloud backend is AVAILABLE"
                : "[HybridAuthService] Cloud backend is NOT reachable - running in OFFLINE mode");
        }
        catch (Exception ex)
        {
            _isCloudAvailable = false;
            Debug.WriteLine($"[HybridAuthService] Initialize error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if cloud backend is accessible
    /// </summary>
    private async Task<bool> CheckCloudConnectivityAsync()
    {
        try
        {
            Debug.WriteLine("[HybridAuthService] Testing cloud connectivity...");
            // Use health endpoint to test connectivity
            var result = await _cloudAuthService.IsHealthyAsync();
            Debug.WriteLine($"[HybridAuthService] Cloud connectivity test result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HybridAuthService] Cloud connectivity test failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Register user - syncs to cloud and local database
    /// </summary>
    public async Task<bool> RegisterAsync(User user)
    {
        // Always try cloud first
        if (_isCloudAvailable)
        {
            var cloudSuccess = await _cloudAuthService.RegisterUserAsync(user);
            if (cloudSuccess)
            {
                // Also save to local database as cache
                await _localDatabase.AddUserAsync(user);
                return true;
            }
        }
        
        // Fallback to local only
        try
        {
            await _localDatabase.AddUserAsync(user);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Login user - checks cloud first, then local database
    /// Keeps both in sync
    /// </summary>
    public async Task<User?> LoginAsync(string email, string password)
    {
        User? user = null;

        // Try cloud first if available
        if (_isCloudAvailable)
        {
            user = await _cloudAuthService.AuthenticateAsync(email, password);
            if (user != null)
            {
                // Update local cache with cloud user
                var existingUser = await _localDatabase.GetUserByEmailAsync(email);
                if (existingUser != null)
                {
                    existingUser.Password = user.Password;
                    await _localDatabase.UpdateUserAsync(existingUser);
                }
                else
                {
                    await _localDatabase.AddUserAsync(user);
                }
                return user;
            }
        }

        // Fallback to local database
        user = await _localDatabase.GetUserAsync(email, password);
        return user;
    }

    /// <summary>
    /// Get user by email - checks cloud first
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (_isCloudAvailable)
        {
            var user = await _cloudAuthService.UserExistsAsync(email) ? 
                new User { Email = email } : null;
            if (user != null)
                return user;
        }

        return await _localDatabase.GetUserByEmailAsync(email);
    }
}
