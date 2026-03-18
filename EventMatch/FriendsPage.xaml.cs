using EventMatch.Models;
using EventMatch.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace EventMatch;

public partial class FriendsPage : ContentPage
{
    public ObservableCollection<Profile> Friends { get; set; } = new();
    public ObservableCollection<Profile> UnaddedUsers { get; set; } = new();
    public ObservableCollection<Profile> PendingRequests { get; set; } = new();
    private readonly UserDatabase _userDb;
    private ObservableCollection<Profile> _allUnaddedUsers = new();
    private System.Timers.Timer _refreshTimer;

    public FriendsPage()
    {
        InitializeComponent();
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<UserDatabase>()!;
        BindingContext = this;
        _refreshTimer = new System.Timers.Timer(5000); // 5 seconds
        _refreshTimer.Elapsed += async (s, e) => await RefreshData();
        _refreshTimer.AutoReset = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshData();
        _refreshTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _refreshTimer.Stop();
    }

    private async Task RefreshData()
    {
        var friends = await _userDb.GetFriendsAsync(Session.CurrentUserEmail);
        var unadded = await _userDb.GetUnaddedUsersAsync(Session.CurrentUserEmail);
        var pending = await _userDb.GetPendingRequestsAsync(Session.CurrentUserEmail);

        Device.BeginInvokeOnMainThread(() =>
        {
            UpdateCollection(Friends, friends);
            UpdateCollection(UnaddedUsers, unadded);
            UpdateCollection(_allUnaddedUsers, unadded);
            UpdateCollection(PendingRequests, pending);
        });
    }

    private void UpdateCollection(ObservableCollection<Profile> collection, IList<Profile> newData)
    {
        var newEmails = newData.Select(p => p.UserEmail).ToHashSet();
        // Remove items not in newData
        for (int i = collection.Count - 1; i >= 0; i--)
        {
            if (!newEmails.Contains(collection[i].UserEmail))
                collection.RemoveAt(i);
        }
        // Add new items
        foreach (var item in newData)
        {
            if (!collection.Any(p => p.UserEmail == item.UserEmail))
                collection.Add(item);
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var search = e.NewTextValue?.ToLower() ?? "";
        UnaddedUsers.Clear();
        var friendUsernames = Friends.Select(f => f.Username.ToLower()).ToHashSet();
        foreach (var user in _allUnaddedUsers)
        {
            if ((user.Username.ToLower().Contains(search) || user.UserEmail.ToLower().Contains(search))
                && !friendUsernames.Contains(user.Username.ToLower()))
            {
                UnaddedUsers.Add(user);
            }
        }
    }

    private void OnAddFriendClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Profile user)
        {
            AddFriend(user);
        }
    }

    public async void AddFriend(Profile user)
    {
        await _userDb.AddFriendAsync(Session.CurrentUserEmail, user.UserEmail);
        UnaddedUsers.Remove(user);
        Friends.Add(user);
    }

    private void OnRemoveFriendClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Profile user)
        {
            RemoveFriend(user);
        }
    }

    public async void RemoveFriend(Profile user)
    {
        await _userDb.RemoveFriendAsync(Session.CurrentUserEmail, user.UserEmail);
        Friends.Remove(user);
        UnaddedUsers.Add(user);
        _allUnaddedUsers.Add(user);
    }

    private void OnAcceptRequestClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Profile user)
        {
            AcceptRequest(user);
        }
    }

    private void OnDeclineRequestClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Profile user)
        {
            DeclineRequest(user);
        }
    }

    public async void AcceptRequest(Profile user)
    {
        await _userDb.AcceptFriendAsync(Session.CurrentUserEmail, user.UserEmail);
        PendingRequests.Remove(user);
        Friends.Add(user);
    }

    public async void DeclineRequest(Profile user)
    {
        await _userDb.DeclineFriendAsync(Session.CurrentUserEmail, user.UserEmail);
        PendingRequests.Remove(user);
        UnaddedUsers.Add(user);
        _allUnaddedUsers.Add(user);
    }

    private async void OnGroupsButtonClicked(object sender, EventArgs e)
    {
        // Use the registered route name to navigate to GroupsPage
        await Shell.Current.GoToAsync("GroupsPage");
    }
}
