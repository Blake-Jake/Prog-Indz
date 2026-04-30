using EventMatch.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System;
using System.IO;
using EventMatch.Models;
using EventMatch.Services;
using Maui.GoogleMaps;

namespace EventMatch;

public partial class EventCreator : ContentPage
{
    double selectedLat;
    double selectedLng;

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

    private string? _selectedAddress;

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Create new event and add to store
        var details = EventDetailsEditor?.Text ?? string.Empty;

        // Find pickers by name (avoids depending on generated fields)
        var datePicker = this.FindByName<DatePicker>("EventDatePicker");
        var timePicker = this.FindByName<TimePicker>("EventTimePicker");

        var date = datePicker?.Date ?? DateTime.Now.Date;
        var time = timePicker?.Time ?? DateTime.Now.TimeOfDay;
        var scheduled = date.Date + time;

        var newEvent = new Event
        {
            Details = details,
            ImageBase64 = _pickedImageBase64 ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            ScheduledAt = scheduled,
            Latitude = selectedLat,
            Longitude = selectedLng,
            LocationAddress = _selectedAddress ?? string.Empty
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

    private async void OnPickLocationClicked(object sender, EventArgs e)
    {
        var mapPage = new EventMapControl();

        mapPage.LocationSelected = async (lat, lng) =>
        {
            selectedLat = lat;
            selectedLng = lng;

#if WINDOWS

            var address = $"{lat:F4}, {lng:F4}";
#else
    var geocoder = new Geocoder();
    var positions = await geocoder.GetAddressesForPositionAsync(new Position(lat, lng));
    var address = positions.FirstOrDefault() ?? $"{lat:F4}, {lng:F4}";
#endif

            _selectedAddress = address;
            LocationLabel.Text = $"📍 {address}";
        };

        await Navigation.PushAsync(mapPage);
    }
}
