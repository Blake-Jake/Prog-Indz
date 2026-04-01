using EventMatch.Models;
using System.Net.Http.Json;
using System.Diagnostics;

namespace EventMatch.Services;

public class CloudAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public CloudAuthService()
    {
        _httpClient = new HttpClient();
        _apiBaseUrl = GetApiBaseUrl();
        Debug.WriteLine($"[CloudAuthService] Using API base URL: {_apiBaseUrl}");
    }

    private static string GetApiBaseUrl()
    {
        // Allow override from environment for testing
        var env = Environment.GetEnvironmentVariable("EVENTMATCH_API_BASE_URL");
        if (!string.IsNullOrEmpty(env)) return env;
        // Default: use remote Render backend
        // If you need to use a local emulator/host, set EVENTMATCH_USE_EMULATOR_LOCAL=1
        var useLocal = Environment.GetEnvironmentVariable("EVENTMATCH_USE_EMULATOR_LOCAL");
       /* if (!string.IsNullOrEmpty(useLocal) && useLocal == "1")
        {
#if ANDROID
            // Android emulator access to host machine
            return "http://10.0.2.2:5000";
#elif WINDOWS
            return "http://localhost:5000";
#else
            return "http://localhost:5000";
#endif
        }*/

        return "https://eventmatch-api.onrender.com";
    }

    /// <summary>
    /// Get all users from cloud (admin endpoint)
    /// </summary>
    public async Task<List<User>?> GetAllUsersAsync()
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/users";
            Debug.WriteLine($"[CloudAuthService] GET {url}");
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var list = await response.Content.ReadFromJsonAsync<List<User>>();
                Debug.WriteLine($"[CloudAuthService] Retrieved {list?.Count ?? 0} users from cloud");
                return list;
            }
            Debug.WriteLine($"[CloudAuthService] GetAllUsers failed: {response.StatusCode} {response.ReasonPhrase}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudAuthService] GetAllUsers error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Register a new user in the cloud
    /// </summary>
    public async Task<bool> RegisterUserAsync(User user)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/auth/register";
            Debug.WriteLine($"[CloudAuthService] POST {url}");
            var response = await _httpClient.PostAsJsonAsync(url, user);
            var success = response.IsSuccessStatusCode;
            Debug.WriteLine($"[CloudAuthService] Register: {(success ? "succeeded" : $"failed {response.StatusCode}")}");
            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudAuthService] Registration error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Authenticate user and retrieve from cloud
    /// </summary>
    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/auth/login";
            Debug.WriteLine($"[CloudAuthService] POST {url}");
            var request = new { email, password };
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<User>();
                Debug.WriteLine($"[CloudAuthService] Authentication succeeded for {email}");
                return user;
            }
            Debug.WriteLine($"[CloudAuthService] Authentication failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudAuthService] Authentication error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Verify if user exists in cloud
    /// </summary>
    public async Task<bool> UserExistsAsync(string email)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/auth/exists/{email}";
            Debug.WriteLine($"[CloudAuthService] GET {url}");
            var response = await _httpClient.GetAsync(url);
            var exists = response.IsSuccessStatusCode;
            Debug.WriteLine($"[CloudAuthService] User exists check: {exists}");
            return exists;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudAuthService] User exists check error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check cloud API health endpoint to determine connectivity
    /// </summary>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/health";
            Debug.WriteLine($"[CloudAuthService] GET {url} (health)");
            var response = await _httpClient.GetAsync(url);
            var healthy = response.IsSuccessStatusCode;
            Debug.WriteLine($"[CloudAuthService] Health check: {response.StatusCode}");
            return healthy;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudAuthService] Health check error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete a user from cloud
    /// </summary>
    public async Task<bool> DeleteUserAsync(string email)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/users/{email}";
            Debug.WriteLine($"[CloudAuthService] DELETE {url}");
            var response = await _httpClient.DeleteAsync(url);
            var success = response.IsSuccessStatusCode;
            Debug.WriteLine($"[CloudAuthService] Delete user {(success ? "succeeded" : $"failed {response.StatusCode}")}");
            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudAuthService] Delete user error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete all users from cloud
    /// </summary>
    public async Task<bool> DeleteAllUsersAsync()
    {
        try
        {
            var users = await GetAllUsersAsync();
            if (users == null || users.Count == 0)
            {
                Debug.WriteLine("[CloudAuthService] No users to delete");
                return true;
            }

            int deleted = 0;
            foreach (var user in users)
            {
                if (await DeleteUserAsync(user.Email))
                {
                    deleted++;
                }
            }
            Debug.WriteLine($"[CloudAuthService] Deleted {deleted} out of {users.Count} users");
            return deleted == users.Count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudAuthService] Delete all users error: {ex.Message}");
            return false;
        }
    }
}
