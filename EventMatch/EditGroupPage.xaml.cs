using EventMatch.Models;

namespace EventMatch;

[QueryProperty(nameof(GroupId), "groupId")]
public partial class EditGroupPage : ContentPage
{
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
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<Services.UserDatabase>()!;
    }

    private async Task LoadGroup()
    {
        if (_groupId == 0) return;
        _group = await _userDb.GetGroupByIdAsync(_groupId);
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
        await _userDb.SaveGroupAsync(_group);
        await Shell.Current.GoToAsync("..", true);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
