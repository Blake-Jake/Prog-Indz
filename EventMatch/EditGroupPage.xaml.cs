using EventMatch.Models;

namespace EventMatch;

[QueryProperty(nameof(GroupId), "groupId")]
public partial class EditGroupPage : ContentPage
{
    private readonly Services.HybridGroupService _groupService;
    private readonly Services.UserDatabase _userDb;
    private int _groupId;
    private Group? _group;

    public int GroupId
    {
        get => _groupId;
        set
        {
            _groupId = value;
            _ = LoadGroup();
        }
    }

    public EditGroupPage()
    {
        InitializeComponent();
        _groupService = Application.Current?.Handler?.MauiContext?.Services.GetService<Services.HybridGroupService>()!;
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<Services.UserDatabase>()!;
    }

    private async Task LoadGroup()
    {
        if (_groupId == 0) return;
        _group = await _groupService.GetGroupByIdAsync(_groupId);
        if (_group == null) return;
        Device.BeginInvokeOnMainThread(() =>
        {
            NameEntry.Text = _group.Name;
            DescriptionEditor.Text = _group.Description;
        });
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_group == null) return;
        _group.Name = NameEntry.Text?.Trim() ?? _group.Name;
        _group.Description = DescriptionEditor.Text?.Trim() ?? _group.Description;
        // Use hybrid service to save (syncs to cloud and local)
        var success = await _groupService.UpdateGroupAsync(_group);
        if (success)
        {
            await Shell.Current.GoToAsync("..", true);
        }
        else
        {
            await DisplayAlert("Error", "Failed to save group", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
