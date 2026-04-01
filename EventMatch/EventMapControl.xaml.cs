using Microsoft.Maui.Controls;
using Maui.GoogleMaps;
using Microsoft.Maui.ApplicationModel;

namespace EventMatch;

public partial class EventMapControl : ContentPage
{
    WebView windowsWeb;
    double defaultLat = 54.8985;
    double defaultLng = 23.9036;
    string defaultLabel = "Kaunas";

    public EventMapControl()
    {
        InitializeComponent();

#if ANDROID
        // Android: Google Maps MAUI
        var map = new Maui.GoogleMaps.Map
        {
            HeightRequest = 500,
            MapType = MapType.Street
        };

        // pridėti pin
        var pin = new Maui.GoogleMaps.Pin
        {
            Label = "Kaunas",
            Address = "Kaunas, Lithuania",
            Type = PinType.Place,
            Position = new Maui.GoogleMaps.Position(54.8985, 23.9036)
        };
        map.Pins.Add(pin);

        // parodome regioną aplink pin
        map.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Maui.GoogleMaps.Position(54.8985, 23.9036),
            Distance.FromKilometers(5)));

        MainLayout.Children.Add(map);

#elif WINDOWS
        // Windows: WebView su iframe HTML
        string html = $@"
<html>
  <body style='margin:0;padding:0;'>
    <iframe
        width='100%'
        height='500'
        frameborder='0'
        style='border:0'
        src='https://www.google.com/maps/embed/v1/view?key=AIzaSyA2lGsQdCDdzQlfhZWYYPVEPye9ixinTvM&center=54.8985,23.9036&zoom=14&maptype=roadmap'
        allowfullscreen>
    </iframe>
  </body>
</html>";

        string embedUrl = $"https://www.google.com/maps/embed/v1/view?key=AIzaSyA2lGsQdCDdzQlfhZWYYPVEPye9ixinTvM&center={defaultLat},{defaultLng}&zoom=14&maptype=roadmap";

        windowsWeb = new WebView
        {
            HeightRequest = 500,
            Source = new HtmlWebViewSource { Html = html }
        };

        MainLayout.Children.Add(windowsWeb);
#endif
    }

    // Jei nori vėliau keisti lokaciją
    public void ShowLocation(double lat, double lng, string label = "Event")
    {
#if ANDROID
        var map = MainLayout.Children[1] as Maui.GoogleMaps.Map;
        map?.Pins.Clear();
        var pin = new Maui.GoogleMaps.Pin
        {
            Label = label,
            Position = new Maui.GoogleMaps.Position(lat, lng),
            Type = PinType.Place
        };
        map?.Pins.Add(pin);
        map?.MoveToRegion(MapSpan.FromCenterAndRadius(new Maui.GoogleMaps.Position(lat, lng), Distance.FromKilometers(5)));
#elif WINDOWS
        if (windowsWeb != null)
        {
            string embedUrl = $"https://www.google.com/maps/embed/v1/view?key=AIzaSyA2lGsQdCDdzQlfhZWYYPVEPye9ixinTvM&center={lat},{lng}&zoom=14&maptype=roadmap";
            windowsWeb.Source = embedUrl;
        }
#endif
    }

}
