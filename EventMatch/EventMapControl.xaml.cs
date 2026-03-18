using Microsoft.Maui.Controls;

namespace EventMatch;

public partial class EventMapControl : ContentPage
{
    public EventMapControl()
    {
        InitializeComponent();
    }

        protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
        {
            EventMap.MyLocationEnabled = true;
        }
        else
        {
            await DisplayAlert("Permission needed", "Location permission is required to show your location.", "OK");
        }
    }
}
