using EventMatch.Services;
using Maui.GoogleMaps;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace EventMatch;

public partial class EventMapControl : ContentPage
{
    public Action<double, double> LocationSelected;

    Maui.GoogleMaps.Map map;

    WebView windowsWeb;
    double defaultLat = 54.8985;
    double defaultLng = 23.9036;
    string defaultLabel = "Kaunas";

#if WINDOWS
    private System.Threading.CancellationTokenSource _pollCts;
#endif

    public EventMapControl()
    {
        InitializeComponent();

#if ANDROID
        var map = new Maui.GoogleMaps.Map
        {
            HeightRequest = 500,
            MapType = MapType.Street
        };

        var pin = new Maui.GoogleMaps.Pin
        {
            Label = "Kaunas",
            Address = "Kaunas, Lithuania",
            Type = PinType.Place,
            Position = new Maui.GoogleMaps.Position(54.8985, 23.9036)
        };
        map.Pins.Add(pin);

        map.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Maui.GoogleMaps.Position(54.8985, 23.9036),
            Distance.FromKilometers(5)));

        map.MapClicked += (s, e) =>
        {
            map.Pins.Clear();

            var newPin = new Pin
            {
                Label = "Selected Event Location",
                Position = e.Point,
                Type = PinType.Place
            };

            map.Pins.Add(newPin);
            LocationSelected?.Invoke(e.Point.Latitude, e.Point.Longitude);
            Navigation.PopAsync();
        };

        MainLayout.Children.Add(map);

#elif WINDOWS
        string mapCss = "#map { width: 100%; height: 500px; }";
        string btnCss = "#confirm-btn { position: absolute; bottom: 20px; left: 50%; transform: translateX(-50%); padding: 10px 24px; background: #2E8B57; color: white; border: none; border-radius: 8px; font-size: 16px; cursor: pointer; z-index: 999; display: none; }";

        string html = $@"
<!DOCTYPE html>
<html>
<head>
  <style>
    body {{ margin: 0; padding: 0; }}
    {mapCss}
    {btnCss}
  </style>
</head>
<body>
  <div id='map'></div>
  <button id='confirm-btn' onclick='confirmLocation()'>Confirm Location</button>
  <script>
    var selectedLat = null;
    var selectedLng = null;
    var marker = null;

    function initMap() {{
      var map = new google.maps.Map(document.getElementById('map'), {{
        center: {{ lat: {defaultLat}, lng: {defaultLng} }},
        zoom: 14
      }});

      map.addListener('click', function(e) {{
        selectedLat = e.latLng.lat();
        selectedLng = e.latLng.lng();

        if (marker) marker.setMap(null);
        marker = new google.maps.Marker({{
          position: e.latLng,
          map: map,
          title: 'Selected Location'
        }});

        document.getElementById('confirm-btn').style.display = 'block';
      }});
    }}

    function confirmLocation() {{
      if (selectedLat !== null && selectedLng !== null) {{
        window.location.href = 'eventmatch://location?lat=' + selectedLat + '&lng=' + selectedLng;
      }}
    }}
  </script>
  <script src='https://maps.googleapis.com/maps/api/js?key=AIzaSyA2lGsQdCDdzQlfhZWYYPVEPye9ixinTvM&callback=initMap' async defer></script>
</body>
</html>";

        windowsWeb = new WebView
        {
            HeightRequest = 500,
            Source = new HtmlWebViewSource { Html = html }
        };

        windowsWeb.Navigating += OnWindowsMapNavigating;
        windowsWeb.Navigated += (s, e) =>
        {
            if (e?.Url == null) return;
            OnWindowsMapNavigating(s, new WebNavigatingEventArgs(
                WebNavigationEvent.NewPage, null, e.Url));
        };

        MainLayout.Children.Add(windowsWeb);
#endif
    }

#if WINDOWS
    private void OnWindowsMapNavigating(object sender, WebNavigatingEventArgs e)
    {
        if (e?.Url == null) return;
        if (e.Url.StartsWith("eventmatch://location"))
        {
            e.Cancel = true;

            try
            {
                // Manual query string parsing - no System.Web needed
                var url = e.Url;
                var queryStart = url.IndexOf('?');
                if (queryStart == -1) return;

                var query = url.Substring(queryStart + 1);
                var parts = query.Split('&');

                double lat = 0, lng = 0;
                bool gotLat = false, gotLng = false;

                foreach (var part in parts)
                {
                    var kv = part.Split('=');
                    if (kv.Length != 2) continue;

                    if (kv[0] == "lat")
                        gotLat = double.TryParse(kv[1],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out lat);

                    if (kv[0] == "lng")
                        gotLng = double.TryParse(kv[1],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out lng);
                }

                if (gotLat && gotLng)
                {
                    LocationSelected?.Invoke(lat, lng);
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Map navigation error: {ex.Message}");
            }
        }
    }
#endif

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
        map?.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Maui.GoogleMaps.Position(lat, lng),
            Distance.FromKilometers(5)));
#elif WINDOWS
        if (windowsWeb != null)
        {
            string embedUrl = $"https://www.google.com/maps/embed/v1/view?key=AIzaSyA2lGsQdCDdzQlfhZWYYPVEPye9ixinTvM&center={lat},{lng}&zoom=14&maptype=roadmap";
            windowsWeb.Source = embedUrl;
        }
#endif
    }
}