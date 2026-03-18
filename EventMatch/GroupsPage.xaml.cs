using EventMatch.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EventMatch;

public partial class GroupsPage : ContentPage
{
    public ObservableCollection<Group> Groups { get; } = new();
    private readonly Services.UserDatabase _userDb;

    public GroupsPage()
    {
        InitializeComponent();
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<Services.UserDatabase>()!;

        BindingContext = this;
    }

    private async void OnCreateGroupClicked(object sender, EventArgs e)
    {
        var name = NewGroupName.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            DisplayAlert("Error", "Group name is required", "OK");
            return;
        }

        var desc = NewGroupDescription.Text?.Trim() ?? string.Empty;
        var g = new Group { Name = name, Description = desc, MemberCount = 1, OwnerEmail = Models.Session.CurrentUserEmail };

        // persist and reload
        await _userDb.SaveGroupAsync(g);
        await LoadGroups();

        NewGroupName.Text = string.Empty;
        NewGroupDescription.Text = string.Empty;
    }

    private async void OnJoinClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Group g)
        {
            // add membership in DB and refresh
            await _userDb.AddUserToGroupAsync(g.Id, Models.Session.CurrentUserEmail);
            await LoadGroups();
        }
    }

    private async void OnEditGroupClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Group g)
        {
            // navigate to EditGroupPage
            await Shell.Current.GoToAsync($"EditGroupPage?groupId={g.Id}");
        }
    }

    private async void OnDeleteGroupClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Group g)
        {
            var ok = await DisplayAlert("Delete", $"Delete group '{g.Name}'?", "Yes", "No");
            if (!ok) return;
            await _userDb.DeleteGroupAsync(g.Id);
            await LoadGroups();
        }
    }

    private async void OnOpenChatClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Group g)
        {
            // navigate to chat page; register route 'GroupChatPage' in MauiProgram if needed
            await Shell.Current.GoToAsync($"GroupChatPage?groupId={g.Id}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadGroups();
    }

    private async Task LoadGroups()
    {
        var list = await _userDb.GetGroupsAsync();
        Device.BeginInvokeOnMainThread(async () =>
        {
            Groups.Clear();
            foreach (var g in list)
            {
                g.IsOwner = g.OwnerEmail == Models.Session.CurrentUserEmail;
                g.IsMember = await _userDb.IsUserMemberOfGroupAsync(g.Id, Models.Session.CurrentUserEmail);
                Groups.Add(g);
            }
        });
    }
}
