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
    private bool _pinsLoaded = false;
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
            if (LocationSelected == null) return;

            map.Pins.Clear();
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
        map.PinClicked += (s, e) =>
        {
            e.Handled = true; // stops default info window

            var tappedPin = e.Pin;
            // find the matching event by position
            var store = new EventStore();
            var ev = store.LoadAll().FirstOrDefault(x =>
                Math.Abs(x.Latitude - tappedPin.Position.Latitude) < 0.0001 &&
                Math.Abs(x.Longitude - tappedPin.Position.Longitude) < 0.0001);

            if (ev == null) return;

            ShowEventOverlay(ev);
        };
        MainLayout.Children.Add(map);

#elif WINDOWS
        string mapCss = "#map { width: 100%; height: 500px; }";
        string btnCss = "#confirm-btn { position: absolute; bottom: 20px; left: 50%; transform: translateX(-50%); padding: 10px 24px; background: #2E8B57; color: white; border: none; border-radius: 8px; font-size: 16px; cursor: pointer; z-index: 999; display: none; }";
        string infoCss = "#info-box { display: none; position: absolute; top: 60px; left: 50%; transform: translateX(-50%); background: rgba(30,30,30,0.95); color: white; padding: 12px 18px; border-radius: 10px; z-index: 998; min-width: 250px; max-width: 320px; text-align: center; box-shadow: 0 2px 8px rgba(0,0,0,0.4); } #info-box .info-title { font-weight: bold; font-size: 15px; margin-bottom: 4px; } #info-box .info-address { font-size: 13px; color: #D8BFD8; } #info-close { float: right; cursor: pointer; margin-left: 10px; font-size: 16px; }";
        string html = $@"
<!DOCTYPE html>
<html>
<head>
  <style>
    body {{ margin: 0; padding: 0; }}
    {mapCss}
    {btnCss}
    {infoCss}
  </style>
</head>
<body>
  <div id='map'></div>
  <button id='confirm-btn' onclick='confirmLocation()'>Confirm Location</button>
  <div id='info-box'>
    <span id='info-close' onclick='closeInfo()'>✕</span>
    <img id='info-img' src='' style='width:100%;max-height:120px;object-fit:cover;border-radius:6px;margin-bottom:8px;display:none;'/>
    <div class='info-title' id='info-title'></div>
    <div class='info-address' id='info-address'></div>
  </div>
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

      pendingPins.forEach(function(p) {{
        addEventPinNow(p.lat, p.lng, p.label, p.address, p.img);
      }});
      pendingPins = [];

      mapObj.addListener('click', function(e) {{
        if (!pickMode) return;
        closeInfo();
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

    function addEventPinNow(lat, lng, label, address, img) {{
      var m = new google.maps.Marker({{
        position: {{ lat: lat, lng: lng }},
        map: mapObj,
        title: label,
        icon: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'
      }});
      m.addListener('click', function() {{
        document.getElementById('info-title').innerText = label;
        document.getElementById('info-address').innerText = address || '';
        var imgEl = document.getElementById('info-img');
        if (img && img.length > 0) {{
          imgEl.src = 'data:image/jpeg;base64,' + img;
          imgEl.style.display = 'block';
        }} else {{
          imgEl.style.display = 'none';
        }}
        document.getElementById('info-box').style.display = 'block';
      }});
    }}

    function addEventPin(lat, lng, label, address, img) {{
      if (mapReady) {{
        addEventPinNow(lat, lng, label, address, img);
      }} else {{
        pendingPins.push({{ lat: lat, lng: lng, label: label, address: address, img: img }});
      }}
    }}

    function closeInfo() {{
      document.getElementById('info-box').style.display = 'none';
    }}

    function enablePickMode() {{
      pickMode = true;
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
            if (_pinsLoaded) return;

            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(500);
                var ready = await windowsWeb.EvaluateJavaScriptAsync("typeof mapObj !== 'undefined' && mapObj !== null ? 'true' : 'false'");
                if (ready == "true")
                {
                    _pinsLoaded = true;
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
        await Task.Delay(300);
        LoadEventPinsAndroid();
#endif
    }

#if ANDROID
    private void LoadEventPinsAndroid()
    {
        if (map == null) return;

        var store = new EventStore();
        var events = store.LoadAll();

        map.Pins.Clear();

        foreach (var ev in events)
        {
            if (ev.Latitude == 0 && ev.Longitude == 0) continue;

            map.Pins.Add(new Pin
            {
                Label = string.IsNullOrEmpty(ev.LocationAddress) ? "Event" : ev.LocationAddress,
                Address = string.IsNullOrEmpty(ev.Details) ? "No description" : ev.Details.Split('\n').FirstOrDefault() ?? "",
                Position = new Maui.GoogleMaps.Position(ev.Latitude, ev.Longitude),
                Type = PinType.Place
            });
        }
    }

    private void ShowEventOverlay(Event ev)
    {
        // remove any existing overlay
        var existing = MainLayout.Children.FirstOrDefault(v => v is Frame f && (string)f.AutomationId == "event-overlay");
        if (existing != null) MainLayout.Children.Remove(existing);

        var frame = new Frame
        {
            AutomationId = "event-overlay",
            BackgroundColor = Color.FromArgb("#EE1e1e1e"),
            CornerRadius = 12,
            Padding = 10,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            WidthRequest = 260,
            Margin = new Thickness(0, 60, 0, 0)
        };

        var stack = new VerticalStackLayout { Spacing = 6 };

        if (!string.IsNullOrEmpty(ev.ImageBase64))
        {
            try
            {
                var bytes = Convert.FromBase64String(ev.ImageBase64);
                stack.Children.Add(new Image
                {
                    Source = ImageSource.FromStream(() => new MemoryStream(bytes)),
                    HeightRequest = 120,
                    Aspect = Aspect.AspectFill
                });
            }
            catch { }
        }

        stack.Children.Add(new Label
        {
            Text = ev.LocationAddress ?? "Event",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 14
        });

        if (!string.IsNullOrEmpty(ev.Details))
            stack.Children.Add(new Label
            {
                Text = ev.Details.Split('\n').FirstOrDefault(),
                TextColor = Color.FromArgb("#CCCCCC"),
                FontSize = 12
            });

        var closeBtn = new Button
        {
            Text = "✕",
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.End,
            HeightRequest = 30,
            WidthRequest = 30,
            Padding = 0
        };
        closeBtn.Clicked += (s, e) => MainLayout.Children.Remove(frame);
        stack.Children.Add(closeBtn);

        frame.Content = stack;
        MainLayout.Children.Add(frame);
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

            var label = (ev.LocationAddress ?? "Event").Replace("'", "\\'");
            var address = (ev.Details?.Split('\n').FirstOrDefault() ?? "No description").Replace("'", "\\'");
            var imageData = string.IsNullOrEmpty(ev.ImageBase64) ? "" : ev.ImageBase64;

            await windowsWeb.EvaluateJavaScriptAsync(
                $"addEventPin(" +
                $"{ev.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
                $"{ev.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
                $"'{label}', '{address}', '{imageData}')");
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
        _pinsLoaded = false;
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