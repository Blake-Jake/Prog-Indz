using EventMatch.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EventMatch;

[QueryProperty(nameof(GroupId), "groupId")]
public partial class GroupChatPage : ContentPage
{
    private readonly Services.HybridGroupService _groupService;
    private readonly Services.UserDatabase _userDb;
    private int _groupId;

    public ObservableCollection<GroupMessage> Messages { get; } = new();

    public GroupChatPage()
    {
        InitializeComponent();
        _groupService = Application.Current?.Handler?.MauiContext?.Services.GetService<Services.HybridGroupService>()!;
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<Services.UserDatabase>()!;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Messages will be loaded when GroupId is set (via QueryProperty) or here if already set
        await LoadMessages();
    }

    private async Task LoadMessages()
    {
        if (_groupId == 0) return;
        // Use hybrid service to get messages (cloud first, local fallback)
        var list = await _groupService.GetGroupMessagesAsync(_groupId);
        Device.BeginInvokeOnMainThread(() =>
        {
            Messages.Clear();
            foreach (var m in list)
                Messages.Add(m);
        });
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var text = NewMessageEntry.Text?.Trim();
        if (string.IsNullOrEmpty(text)) return;
        if (_groupId == 0) return;
        var msg = new GroupMessage { GroupId = _groupId, FromEmail = Models.Session.CurrentUserEmail, Text = text };
        // Use hybrid service to add message (syncs to cloud and local)
        var success = await _groupService.AddGroupMessageAsync(msg);
        if (success)
        {
            NewMessageEntry.Text = string.Empty;
            await LoadMessages();
        }
    }

    public int GroupId
    {
        get => _groupId;
        set
        {
            _groupId = value;
            _ = LoadMessages();
        }
    }
}
