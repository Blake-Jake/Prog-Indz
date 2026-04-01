using EventMatch.Models;
using EventMatch.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using SQLite;

namespace EventMatch;

[QueryProperty(nameof(Email), "email")]
public partial class ProfilePage : ContentPage
{
    private readonly UserDatabase _userDb;
    private EventMatch.Models.Profile? _currentProfile;
    UploadingImage uploadImage { get; set; }

    private string _photoBase64 = "";

    [PrimaryKey, AutoIncrement]
    public new int Id { get; set; }

    [Unique]
    public string UserEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public int RadiusKm { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;

    public ProfilePage()
    {
        InitializeComponent();
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<UserDatabase>()!;

        if (RadiusPicker != null && RadiusPicker.Items.Count > 0)
            RadiusPicker.SelectedIndex = 0;

        uploadImage = new UploadingImage();
        // Make the profile image area tappable to change/add photo
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnAddPhotoClicked;
        ProfileFrame.GestureRecognizers.Add(tap);
    }

    // email is passed in navigation as ?email=someone@example.com
    public string Email { get; set; } = string.Empty;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (string.IsNullOrWhiteSpace(Email))
            Email = Session.CurrentUserEmail;

        if (string.IsNullOrWhiteSpace(Email))
            return;

        _currentProfile = await _userDb.GetProfileByEmailAsync(Email);

        if (_currentProfile != null)
        {
            UsernameEntry.Text = _currentProfile.Username;
            TagEntry.Text = _currentProfile.Tag;
            DescriptionEditor.Text = _currentProfile.Description;

            if (!string.IsNullOrEmpty(_currentProfile.PhotoPath))
            {
                var bytes = Convert.FromBase64String(_currentProfile.PhotoPath);
                ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                PhotoOverlayLabel.IsVisible = false;
            }
            else
            {
                ProfileImage.Source = "profile-placeholder.png";
                PhotoOverlayLabel.IsVisible = true;
                PhotoOverlayLabel.Text = "Add Photo";
            }

            var index = RadiusPicker.Items.IndexOf(_currentProfile.RadiusKm.ToString());
            RadiusPicker.SelectedIndex = index >= 0 ? index : 0;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnFriendsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("FriendsPage");
    }

    private async void OnMyEventsClicked(object sender, EventArgs e)
    {
        // Navigate to a page that shows all events (new page)
        await Shell.Current.GoToAsync("AllEventsPage");
    }

    private async void OnAddPhotoClicked(object sender, EventArgs e)
    {

        var img = await uploadImage.OpenMediaPickerAsync();
        if (img == null)
            return;

        var imageFile = await uploadImage.Upload(img);

        _photoBase64 = imageFile.ByteBase64;

        var bytes = Convert.FromBase64String(_photoBase64);
        ProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
        PhotoOverlayLabel.IsVisible = false;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Email))
            Email = Session.CurrentUserEmail;

        if (string.IsNullOrWhiteSpace(Email))
        {
            await DisplayAlertAsync("Error", "No user is logged in.", "OK");
            return;
        }

        var radius = 10;
        if (RadiusPicker.SelectedIndex >= 0 && int.TryParse(RadiusPicker.Items[RadiusPicker.SelectedIndex], out var r))
            radius = r;

        var profileToSave = new Profile
        {
            UserEmail = Email,
            Username = UsernameEntry.Text?.Trim() ?? string.Empty,
            Tag = TagEntry.Text?.Trim() ?? string.Empty,
            RadiusKm = radius,
            Description = DescriptionEditor.Text ?? string.Empty,
            PhotoPath = string.IsNullOrEmpty(_photoBase64)
                ? _currentProfile?.PhotoPath ?? ""
                : _photoBase64
        };

        await _userDb.SaveProfileAsync(profileToSave);

        _currentProfile = await _userDb.GetProfileByEmailAsync(Email);
        await DisplayAlertAsync("Saved", "Profile saved.", "OK");
    }

    private async void OnDeleteAllDataClicked(object sender, EventArgs e)
    {
        // Double confirmation - user must be very sure
        bool firstConfirm = await DisplayAlert(
            "⚠️ WARNING",
            "Delete ALL users and groups from Cloud AND Local?\n\nThis action CANNOT be undone!",
            "Yes, Delete Everything",
            "Cancel"
        );

        if (!firstConfirm) return;

        // Second confirmation
        bool secondConfirm = await DisplayAlert(
            "🔥 FINAL WARNING",
            "This is your LAST chance. Are you ABSOLUTELY sure?",
            "YES, DELETE EVERYTHING",
            "Cancel"
        );

        if (!secondConfirm) return;

        try
        {
            // Show loading indicator
            await DisplayAlert("⏳ Deleting...", "Please wait, this may take a moment...", "");

            // Get HybridGroupService from DI
            var hybridGroupService = Application.Current?.Handler?.MauiContext?.Services.GetService<HybridGroupService>();

            if (hybridGroupService == null)
            {
                await DisplayAlert("❌ Error", "HybridGroupService not available", "OK");
                return;
            }

            // Delete all data
            bool success = await hybridGroupService.DeleteAllDataAsync();

            await DisplayAlert(
                success ? "✅ SUCCESS" : "⚠️ Partial Success",
                success
                    ? "All users and groups deleted from Cloud and Local database!"
                    : "Deletion completed with some warnings. Check debug logs.",
                "OK"
            );

            if (success)
            {
                // Clear session and return to login
                Session.CurrentUserEmail = "";
                await Shell.Current.GoToAsync("///login");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Error", $"Deletion failed:\n{ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[ProfilePage] Delete error: {ex}");
        }
    }
}