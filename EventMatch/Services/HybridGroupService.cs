using EventMatch.Models;
using System.Diagnostics;
using System;

namespace EventMatch.Services;

/// <summary>
/// Hybrid group service that syncs between cloud and local database
/// Groups are synced just like user authentication
/// </summary>
public class HybridGroupService
{
    private readonly CloudGroupService _cloudGroupService;
    private readonly CloudAuthService _cloudAuthService;
    private readonly UserDatabase _localDatabase;
    private bool _isCloudAvailable = true;
    private bool _cloudOnly = false;  // ✅ Default to false - delete local data by default

    public HybridGroupService(CloudGroupService cloudGroupService, UserDatabase localDatabase, CloudAuthService cloudAuthService)
    {
        _cloudGroupService = cloudGroupService;
        _localDatabase = localDatabase;
        _cloudAuthService = cloudAuthService;

        try
        {
            var val = System.Environment.GetEnvironmentVariable("EVENTMATCH_CLOUD_ONLY");
            _cloudOnly = !string.IsNullOrEmpty(val) && (val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase));
            Debug.WriteLine($"[HybridGroupService] Cloud-only mode: {_cloudOnly}");
        }
        catch { }
    }

    /// <summary>
    /// Initialize cloud connectivity check
    /// </summary>
    public async Task InitializeAsync()
    {
        _isCloudAvailable = await CheckCloudConnectivityAsync();
        Debug.WriteLine($"[HybridGroupService] Cloud available: {_isCloudAvailable}");
    }

    /// <summary>
    /// Push local users and groups to cloud backend
    /// </summary>
    public async Task SyncLocalToCloudAsync()
    {
        Debug.WriteLine("[HybridGroupService] SyncLocalToCloudAsync is disabled: local users/groups will NOT be synced to cloud.");
        return;
    }

    /// <summary>
    /// Check if cloud backend is accessible
    /// </summary>
    private async Task<bool> CheckCloudConnectivityAsync()
    {
        try
        {
            Debug.WriteLine("[HybridGroupService] Testing cloud connectivity...");
            // Use cloud auth service health endpoint to check availability
            var healthy = await _cloudAuthService.IsHealthyAsync();
            Debug.WriteLine($"[HybridGroupService] Cloud health check: {healthy}");
            return healthy;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HybridGroupService] Cloud connectivity test failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Create group - syncs to cloud and local database
    /// </summary>
    public async Task<Group?> CreateGroupAsync(Group group)
    {
        Debug.WriteLine($"[HybridGroupService] CreateGroupAsync: {group.Name}, cloud available: {_isCloudAvailable}");
        Group? createdGroup = null;

        // Try cloud first if available
        if (_isCloudAvailable)
        {
            createdGroup = await _cloudGroupService.CreateGroupAsync(group);
            if (createdGroup != null)
            {
                Debug.WriteLine($"[HybridGroupService] Group created on cloud with ID: {createdGroup.Id}");
                // Ensure local copy has CloudId set so we dedupe properly
                createdGroup.CloudId = createdGroup.Id;
                // Also save to local database as cache
                await _localDatabase.SaveGroupAsync(createdGroup);
                return createdGroup;
            }
            else
            {
                Debug.WriteLine($"[HybridGroupService] Cloud creation failed, falling back to local");
            }
        }

        // Fallback to local only
        try
        {
            Debug.WriteLine($"[HybridGroupService] Saving group to local database only");
            var id = await _localDatabase.SaveGroupAsync(group);
            group.Id = (int)id;
            Debug.WriteLine($"[HybridGroupService] Group saved locally with ID: {id}");
            return group;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get all groups for user - syncs cloud and local data
    /// </summary>
    public async Task<List<Group>> GetUserGroupsAsync(string userEmail)
    {
        var allGroups = new Dictionary<int, Group>();
        Debug.WriteLine($"[HybridGroupService] GetUserGroupsAsync called for: {userEmail}");

        // Get local user groups first
        try
        {
            var localGroups = await _localDatabase.GetUserGroupsAsync(userEmail);
            Debug.WriteLine($"[HybridGroupService] Found {localGroups.Count} local groups for {userEmail}");
            foreach (var group in localGroups)
            {
                allGroups[group.Id] = group;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HybridGroupService] Error getting local groups: {ex.Message}");
        }

        // Try to sync with cloud if available
        if (_isCloudAvailable)
        {
            try
            {
                Debug.WriteLine($"[HybridGroupService] Cloud is available, attempting to sync...");
                var cloudGroups = await _cloudGroupService.GetUserGroupsAsync(userEmail);
                if (cloudGroups != null && cloudGroups.Count > 0)
                {
                    Debug.WriteLine($"[HybridGroupService] Got {cloudGroups.Count} groups from cloud");
                    // Sync cloud groups to local database
                    foreach (var group in cloudGroups)
                    {
                        try
                        {
                            var existingGroup = await _localDatabase.GetGroupByIdAsync(group.Id);
                            if (existingGroup == null)
                            {
                                await _localDatabase.SaveGroupAsync(group);
                            }
                            // Always use cloud data if available (it's more recent)
                            allGroups[group.Id] = group;
                        }
                        catch { }
                    }
                }
                else
                {
                    Debug.WriteLine($"[HybridGroupService] Cloud returned no groups");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridGroupService] Error syncing with cloud: {ex.Message}");
            }
        }
        else
        {
            Debug.WriteLine($"[HybridGroupService] Cloud is NOT available, using local groups only");
        }

        Debug.WriteLine($"[HybridGroupService] Returning {allGroups.Count} total groups");
        return allGroups.Values.ToList();
    }

    /// <summary>
    /// Add user to group - syncs to cloud and local
    /// </summary>
    public async Task<bool> AddUserToGroupAsync(int groupId, string userEmail)
    {
        bool success = false;

        // Try cloud first
        if (_isCloudAvailable)
        {
            success = await _cloudGroupService.AddUserToGroupAsync(groupId, userEmail);
            if (success)
            {
                // Also update local database
                await _localDatabase.AddUserToGroupAsync(groupId, userEmail);
                return true;
            }
        }

        // Fallback to local only
        try
        {
            await _localDatabase.AddUserToGroupAsync(groupId, userEmail);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get group by ID - checks cloud first
    /// </summary>
    public async Task<Group?> GetGroupByIdAsync(int groupId)
    {
        // Try cloud first
        if (_isCloudAvailable)
        {
            // Cloud API would need a specific endpoint for this
            // For now, use local
        }

        return await _localDatabase.GetGroupByIdAsync(groupId);
    }

    /// <summary>
    /// Update group - syncs to cloud and local
    /// </summary>
    public async Task<bool> UpdateGroupAsync(Group group)
    {
        bool success = false;

        // Try cloud first
        if (_isCloudAvailable)
        {
            success = await _cloudGroupService.UpdateGroupAsync(group);
            if (success)
            {
                // Also update local database
                await _localDatabase.SaveGroupAsync(group);
                return true;
            }
        }

        // Fallback to local only
        try
        {
            await _localDatabase.SaveGroupAsync(group);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Delete group - removes from cloud and local
    /// </summary>
    public async Task<bool> DeleteGroupAsync(int groupId)
    {
        bool success = false;

        // Try cloud first
        if (_isCloudAvailable)
        {
            success = await _cloudGroupService.DeleteGroupAsync(groupId);
        }

        // Also delete from local
        try
        {
            await _localDatabase.DeleteGroupAsync(groupId);
            return true;
        }
        catch
        {
            return success;
        }
    }

    /// <summary>
    /// Get group messages - checks cloud first
    /// </summary>
    public async Task<List<GroupMessage>> GetGroupMessagesAsync(int groupId)
    {
        // Try cloud first
        if (_isCloudAvailable)
        {
            var cloudMessages = await _cloudGroupService.GetGroupMessagesAsync(groupId);
            if (cloudMessages != null && cloudMessages.Count > 0)
            {
                return cloudMessages;
            }
        }

        // Fallback to local
        return await _localDatabase.GetMessagesForGroupAsync(groupId);
    }

    /// <summary>
    /// Add group message - syncs to cloud and local
    /// </summary>
    public async Task<bool> AddGroupMessageAsync(GroupMessage message)
    {
        bool success = false;

        // Try cloud first
        if (_isCloudAvailable)
        {
            success = await _cloudGroupService.AddGroupMessageAsync(message);
            if (success)
            {
                await _localDatabase.AddGroupMessageAsync(message);
                return true;
            }
        }

        // Fallback to local
        try
        {
            await _localDatabase.AddGroupMessageAsync(message);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Delete all users and groups from both cloud and local database
    /// WARNING: This is destructive and cannot be undone!
    /// </summary>
    public async Task<bool> DeleteAllDataAsync()
    {
        bool cloudSuccess = true;
        bool localSuccess = true;

        Debug.WriteLine("[HybridGroupService] WARNING: Deleting all data from cloud and local database!");

        // Delete from cloud first
        if (_isCloudAvailable)
        {
            try
            {
                Debug.WriteLine("[HybridGroupService] Deleting all cloud groups...");
                await _cloudGroupService.DeleteAllGroupsAsync();

                Debug.WriteLine("[HybridGroupService] Deleting all cloud users...");
                await _cloudAuthService.DeleteAllUsersAsync();

                Debug.WriteLine("[HybridGroupService] Cloud data deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridGroupService] Error deleting cloud data: {ex.Message}");
                cloudSuccess = false;
            }
        }

        // Delete from local database (unless cloud-only mode)
        if (!_cloudOnly)
        {
            try
            {
                Debug.WriteLine("[HybridGroupService] Deleting all local data...");
                await _localDatabase.ClearAllDataAsync();
                Debug.WriteLine("[HybridGroupService] Local data deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridGroupService] Error deleting local data: {ex.Message}");
                localSuccess = false;
            }
        }
        else
        {
            Debug.WriteLine("[HybridGroupService] Cloud-only mode enabled - skipping local data deletion");
        }

        return cloudSuccess && localSuccess;
    }

    /// <summary>
    /// Delete all groups (cloud and local)
    /// </summary>
    public async Task<bool> DeleteAllGroupsAsync()
    {
        bool cloudSuccess = true;
        bool localSuccess = true;

        Debug.WriteLine("[HybridGroupService] Deleting all groups...");

        // Delete from cloud
        if (_isCloudAvailable)
        {
            try
            {
                Debug.WriteLine("[HybridGroupService] Deleting all cloud groups...");
                await _cloudGroupService.DeleteAllGroupsAsync();
                Debug.WriteLine("[HybridGroupService] Cloud groups deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridGroupService] Error deleting cloud groups: {ex.Message}");
                cloudSuccess = false;
            }
        }

        // Delete from local (unless cloud-only mode)
        if (!_cloudOnly)
        {
            try
            {
                Debug.WriteLine("[HybridGroupService] Deleting all local groups...");
                await _localDatabase.DeleteAllGroupsAsync();
                Debug.WriteLine("[HybridGroupService] Local groups deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridGroupService] Error deleting local groups: {ex.Message}");
                localSuccess = false;
            }
        }
        else
        {
            Debug.WriteLine("[HybridGroupService] Cloud-only mode enabled - skipping local groups deletion");
        }

        return cloudSuccess && localSuccess;
    }

    /// <summary>
    /// Delete all users (cloud and local)
    /// </summary>
    public async Task<bool> DeleteAllUsersAsync()
    {
        bool cloudSuccess = true;
        bool localSuccess = true;

        Debug.WriteLine("[HybridGroupService] Deleting all users...");

        // Delete from cloud
        if (_isCloudAvailable)
        {
            try
            {
                Debug.WriteLine("[HybridGroupService] Deleting all cloud users...");
                await _cloudAuthService.DeleteAllUsersAsync();
                Debug.WriteLine("[HybridGroupService] Cloud users deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridGroupService] Error deleting cloud users: {ex.Message}");
                cloudSuccess = false;
            }
        }

        // Delete from local (unless cloud-only mode)
        if (!_cloudOnly)
        {
            try
            {
                Debug.WriteLine("[HybridGroupService] Deleting all local users...");
                await _localDatabase.DeleteAllUsersAsync();
                Debug.WriteLine("[HybridGroupService] Local users deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HybridGroupService] Error deleting local users: {ex.Message}");
                localSuccess = false;
            }
        }
        else
        {
            Debug.WriteLine("[HybridGroupService] Cloud-only mode enabled - skipping local users deletion");
        }

        return cloudSuccess && localSuccess;
    }
}
