using EventMatch.Services;
using Maui.GoogleMaps;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using EventMatch.Models;

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
        map = new Maui.GoogleMaps.Map
        {
            HeightRequest = 500,
            MapType = MapType.Street
        };

        map.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Maui.GoogleMaps.Position(defaultLat, defaultLng),
            Distance.FromKilometers(5)));

        map.MapClicked += (s, e) =>
        {
            // Only allow picking if in pick mode
            if (LocationSelected == null) return;

            map.Pins.Clear();
            // Re-add event pins
            LoadEventPinsAndroid();

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
    var confirmed = false;
    var marker = null;
    var mapObj = null;
    var pickMode = false;
    var pendingPins = [];
    var mapReady = false;

    function initMap() {{
      mapObj = new google.maps.Map(document.getElementById('map'), {{
        center: {{ lat: {defaultLat}, lng: {defaultLng} }},
        zoom: 12
      }});

      mapReady = true;

      // Add any pins that were queued before map was ready
      pendingPins.forEach(function(p) {{
        new google.maps.Marker({{
          position: {{ lat: p.lat, lng: p.lng }},
          map: mapObj,
          title: p.label,
          icon: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'
        }});
      }});
      pendingPins = [];

      mapObj.addListener('click', function(e) {{
        if (!pickMode) return;
        selectedLat = e.latLng.lat();
        selectedLng = e.latLng.lng();
        confirmed = false;

        if (marker) marker.setMap(null);
        marker = new google.maps.Marker({{
          position: e.latLng,
          map: mapObj,
          title: 'Selected Location'
        }});

        document.getElementById('confirm-btn').style.display = 'block';
      }});
    }}

    function enablePickMode() {{
      pickMode = true;
    }}

    function addEventPin(lat, lng, label) {{
      if (mapReady) {{
        new google.maps.Marker({{
          position: {{ lat: lat, lng: lng }},
          map: mapObj,
          title: label,
          icon: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'
        }});
      }} else {{
        pendingPins.push({{ lat: lat, lng: lng, label: label }});
      }}
    }}

    function confirmLocation() {{
      if (selectedLat !== null && selectedLng !== null) {{
        confirmed = true;
      }}
    }}

    function getResult() {{
      if (confirmed) {{
        return selectedLat + ',' + selectedLng;
      }}
      return '';
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

        windowsWeb.Navigated += async (s, e) =>
        {
            // Poll until Google Maps JS is actually ready
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(500);
                var ready = await windowsWeb.EvaluateJavaScriptAsync("typeof mapObj !== 'undefined' && mapObj !== null ? 'true' : 'false'");
                if (ready == "true")
                {
                    StartPolling();
                    await LoadEventPinsWindows();
                    if (LocationSelected != null)
                        await windowsWeb.EvaluateJavaScriptAsync("enablePickMode()");
                    break;
                }
            }
        };

        MainLayout.Children.Add(windowsWeb);
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
#if ANDROID
        await Task.Delay(300); // wait for map to render
        LoadEventPinsAndroid();
#endif
    }

#if ANDROID
    private void LoadEventPinsAndroid()
    {
        if (map == null) return;

        var store = new EventStore();
        var events = store.LoadAll();

        map.Pins.Clear(); // clear first, then re-add all

        foreach (var ev in events)
        {
            if (ev.Latitude == 0 && ev.Longitude == 0) continue;

            map.Pins.Add(new Pin
            {
                Label = string.IsNullOrEmpty(ev.LocationAddress) ? "Event" : ev.LocationAddress,
                Address = ev.Details?.Split('\n').FirstOrDefault() ?? "",
                Position = new Maui.GoogleMaps.Position(ev.Latitude, ev.Longitude),
                Type = PinType.Place
            });
        }
    }
#endif

#if WINDOWS
    private async Task LoadEventPinsWindows()
    {
        if (windowsWeb == null) return;

        var store = new EventStore();
        var events = store.LoadAll();

        foreach (var ev in events)
        {
            if (ev.Latitude == 0 && ev.Longitude == 0) continue;
            var label = (ev.LocationAddress ?? ev.Details?.Split('\n').FirstOrDefault() ?? "Event")
                .Replace("'", "\\'");
            await windowsWeb.EvaluateJavaScriptAsync(
                $"addEventPin({ev.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
                $"{ev.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, '{label}')");
        }
    }

    private void StartPolling()
    {
        _pollCts?.Cancel();
        _pollCts = new System.Threading.CancellationTokenSource();
        var token = _pollCts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(500, token);

                string result = null;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try { result = await windowsWeb.EvaluateJavaScriptAsync("getResult()"); }
                    catch { }
                });

                if (!string.IsNullOrEmpty(result) && result != "null")
                {
                    _pollCts.Cancel();
                    var parts = result.Split(',');
                    if (parts.Length == 2 &&
                        double.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out double lng))
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            LocationSelected?.Invoke(lat, lng);
                            await Navigation.PopAsync();
                        });
                    }
                }
            }
        }, token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _pollCts?.Cancel();
    }
#endif

    public void ShowLocation(double lat, double lng, string label = "Event")
    {
#if ANDROID
        map?.Pins.Clear();
        map?.Pins.Add(new Maui.GoogleMaps.Pin
        {
            Label = label,
            Position = new Maui.GoogleMaps.Position(lat, lng),
            Type = PinType.Place
        });
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