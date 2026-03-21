using EventMatch.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System;
using System.IO;
using EventMatch.Models;
using EventMatch.Services;

namespace EventMatch;

public partial class EventCreator : ContentPage
{
    public EventCreator()
    {
        InitializeComponent();
    }

    private UploadingImage _uploader = new UploadingImage();
    private string? _pickedImageBase64;
    private EventStore _store = new EventStore();

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Create new event and add to store
        var details = EventDetailsEditor?.Text ?? string.Empty;
        var newEvent = new Event
        {
            Details = details,
            ImageBase64 = _pickedImageBase64 ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _store.Add(newEvent);

        await DisplayAlert("Saved", "Event saved.", "OK");
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
