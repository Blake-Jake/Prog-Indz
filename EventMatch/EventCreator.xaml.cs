using EventMatch.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System;
using System.IO;

namespace EventMatch;

public partial class EventCreator : ContentPage
{
    public EventCreator()
    {
        InitializeComponent();
    }

    private UploadingImage _uploader = new UploadingImage();
    private string? _pickedImageBase64;

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Save details and image to simple preferences for preview
        var details = EventDetailsEditor?.Text ?? string.Empty;
        Preferences.Set("LastEvent_Details", details);

        // If an image was already picked, save its base64. Otherwise keep existing preference value.
        if (!string.IsNullOrEmpty(_pickedImageBase64))
        {
            Preferences.Set("LastEvent_ImageBase64", _pickedImageBase64);
        }

        await DisplayAlert("Saved", "Event saved for preview.", "OK");
        await Shell.Current.GoToAsync("EventPreview");
    }

    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        var file = await _uploader.OpenMediaPickerAsync();
        if (file == null)
            return;

        var imageFile = await _uploader.Upload(file);
        if (imageFile == null)
            return;

        _pickedImageBase64 = imageFile.ByteBase64;

        try
        {
            var bytes = Convert.FromBase64String(_pickedImageBase64);
            EventImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            if (ImageOverlayLabel != null)
                ImageOverlayLabel.IsVisible = false;
        }
        catch { }
    }
}
