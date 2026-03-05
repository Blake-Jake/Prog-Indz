using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using EventMatch.Models;
using EventMatch.Services;

namespace EventMatch;

[QueryProperty(nameof(Email), "email")]
public partial class ProfilePage : ContentPage
{
    private readonly UserDatabase _userDb;
    private EventMatch.Models.Profile? _currentProfile;

    public ProfilePage()
    {
        InitializeComponent();
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<UserDatabase>()!;

        if (RadiusPicker != null && RadiusPicker.Items.Count > 0)
            RadiusPicker.SelectedIndex = 0;
    }

    // email is passed in navigation as ?email=someone@example.com
    public string Email { get; set; } = string.Empty;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (string.IsNullOrWhiteSpace(Email))
            return;

        _currentProfile = await _userDb.GetProfileByEmailAsync(Email);
        if (_currentProfile != null)
        {
            UsernameEntry.Text = _currentProfile.Username;
            TagEntry.Text = _currentProfile.Tag;
            DescriptionEditor.Text = _currentProfile.Description;
            ProfileImage.Source = string.IsNullOrEmpty(_currentProfile.PhotoPath) ? "profile-placeholder.png" : _currentProfile.PhotoPath;

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

    private async void OnAddPhotoClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Info", "Photo picker not implemented yet.", "OK");
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            await DisplayAlert("Error", "No user email provided. Navigate to this page with ?email=user@example.com", "OK");
            return;
        }

        var radius = 10;
        if (RadiusPicker.SelectedIndex >= 0 && int.TryParse(RadiusPicker.Items[RadiusPicker.SelectedIndex], out var r))
            radius = r;

        var profileToSave = new EventMatch.Models.Profile
        {
            UserEmail = Email,
            Username = UsernameEntry.Text?.Trim() ?? string.Empty,
            Tag = TagEntry.Text?.Trim() ?? string.Empty,
            RadiusKm = radius,
            Description = DescriptionEditor.Text ?? string.Empty,
            PhotoPath = _currentProfile?.PhotoPath ?? string.Empty
        };

        await _userDb.SaveProfileAsync(profileToSave);
        await DisplayAlert("Saved", "Profile saved.", "OK");
        _currentProfile = await _userDb.GetProfileByEmailAsync(Email);
    }
}