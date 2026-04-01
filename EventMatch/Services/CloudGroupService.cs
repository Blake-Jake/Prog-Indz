using EventMatch.Models;
using System.Net.Http.Json;
using System.Diagnostics;

namespace EventMatch.Services;

/// <summary>
/// Cloud service for group synchronization across emulators
/// </summary>
public class CloudGroupService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public CloudGroupService()
    {
        _httpClient = new HttpClient();
        _apiBaseUrl = GetApiBaseUrl();
        Debug.WriteLine($"[CloudGroupService] Using API base URL: {_apiBaseUrl}");
    }

    /// <summary>
    /// Get all groups from cloud (public browse)
    /// </summary>
    public async Task<List<Group>?> GetAllGroupsAsync()
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/groups";
            Debug.WriteLine($"[CloudGroupService] GET {url} (all groups)");
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<Group>>();
                Debug.WriteLine($"[CloudGroupService] GetAllGroups succeeded: {result?.Count ?? 0} groups");
                return result;
            }
            Debug.WriteLine($"[CloudGroupService] GetAllGroups failed: {response.StatusCode} {response.ReasonPhrase}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudGroupService] GetAllGroups error: {ex.Message}");
            return null;
        }
    }

    private static string GetApiBaseUrl()
    {
        var env = Environment.GetEnvironmentVariable("EVENTMATCH_API_BASE_URL");
        if (!string.IsNullOrEmpty(env)) return env;

        // Allow forcing local emulator/host usage with EVENTMATCH_USE_EMULATOR_LOCAL=1
        var useLocal = Environment.GetEnvironmentVariable("EVENTMATCH_USE_EMULATOR_LOCAL");
       /* if (!string.IsNullOrEmpty(useLocal) && useLocal == "1")
        {
#if ANDROID
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
    /// Create a new group in cloud
    /// </summary>
    public async Task<Group?> CreateGroupAsync(Group group)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/groups/create";
            Debug.WriteLine($"[CloudGroupService] POST {url} for group '{group.Name}'");
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(group)
            };
            // include current user as header for authorization on server
            var current = EventMatch.Models.Session.CurrentUserEmail;
            if (!string.IsNullOrEmpty(current)) req.Headers.Add("x-user-email", current);
            var response = await _httpClient.SendAsync(req);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Group>();
                Debug.WriteLine($"[CloudGroupService] Create group succeeded, ID: {result?.Id}");
                return result;
            }
            Debug.WriteLine($"[CloudGroupService] Create group failed: {response.StatusCode} {response.ReasonPhrase}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudGroupService] Create group error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get all groups for a user from cloud
    /// </summary>
    public async Task<List<Group>?> GetUserGroupsAsync(string userEmail)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/groups/user/{userEmail}";
            Debug.WriteLine($"[CloudGroupService] GET {url}");
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<Group>>();
                Debug.WriteLine($"[CloudGroupService] Get groups succeeded: {result?.Count ?? 0} groups");
                return result;
            }
            Debug.WriteLine($"[CloudGroupService] Get groups failed: {response.StatusCode} {response.ReasonPhrase}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudGroupService] Get groups error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Add user to group in cloud
    /// </summary>
    public async Task<bool> AddUserToGroupAsync(int groupId, string userEmail)
    {
        try
        {
            var request = new { groupId, userEmail };
            var url = $"{_apiBaseUrl}/api/groups/add-member";
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(request)
            };
            var current = EventMatch.Models.Session.CurrentUserEmail;
            if (!string.IsNullOrEmpty(current)) req.Headers.Add("x-user-email", current);
            var response = await _httpClient.SendAsync(req);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Add user to group error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Create user in cloud (register)
    /// </summary>
    public async Task<bool> CreateUserAsync(User user)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/auth/register";
            Debug.WriteLine($"[CloudGroupService] POST {url} to create user {user.Email}");
            var response = await _httpClient.PostAsJsonAsync(url, user);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Create user error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Update group in cloud
    /// </summary>
    public async Task<bool> UpdateGroupAsync(Group group)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/groups/{group.Id}";
            var req = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(group)
            };
            var current = EventMatch.Models.Session.CurrentUserEmail;
            if (!string.IsNullOrEmpty(current)) req.Headers.Add("x-user-email", current);
            var response = await _httpClient.SendAsync(req);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update group error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete group from cloud
    /// </summary>
    public async Task<bool> DeleteGroupAsync(int groupId)
    {
        try
        {
            var url = $"{_apiBaseUrl}/api/groups/{groupId}";
            var req = new HttpRequestMessage(HttpMethod.Delete, url);
            var current = EventMatch.Models.Session.CurrentUserEmail;
            if (!string.IsNullOrEmpty(current)) req.Headers.Add("x-user-email", current);
            var response = await _httpClient.SendAsync(req);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Delete group error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get group messages from cloud
    /// </summary>
    public async Task<List<GroupMessage>?> GetGroupMessagesAsync(int groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/groups/{groupId}/messages");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<GroupMessage>>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get messages error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Add message to group in cloud
    /// </summary>
    public async Task<bool> AddGroupMessageAsync(GroupMessage message)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/api/groups/messages", message);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Add message error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete all groups from cloud
    /// </summary>
    public async Task<bool> DeleteAllGroupsAsync()
    {
        try
        {
            var groups = await GetAllGroupsAsync();
            if (groups == null || groups.Count == 0)
            {
                Debug.WriteLine("[CloudGroupService] No groups to delete");
                return true;
            }

            int deleted = 0;
            foreach (var group in groups)
            {
                if (await DeleteGroupAsync(group.Id))
                {
                    deleted++;
                }
            }
            Debug.WriteLine($"[CloudGroupService] Deleted {deleted} out of {groups.Count} groups");
            return deleted == groups.Count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CloudGroupService] Delete all groups error: {ex.Message}");
            return false;
        }
    }
}
