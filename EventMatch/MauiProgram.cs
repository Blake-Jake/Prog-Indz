using EventMatch.Services;
using System.IO;
using Microsoft.Maui.Controls;
using Maui.GoogleMaps.Hosting;
using Maui.GoogleMaps;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace EventMatch
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
#if WINDOWS
Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
{
    var nativeWindow = handler.PlatformView;
    nativeWindow.Activate();

    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
    var appWindow = AppWindow.GetFromWindowId(windowId);

    // telefono dydis
    appWindow.Resize(new SizeInt32(500, 990));

    // užrakina resize (nebūtina)
    var presenter = appWindow.Presenter as OverlappedPresenter;
    if (presenter != null)
    {
        presenter.IsResizable = false;
        presenter.IsMaximizable = false;
    }
});
#endif
            builder
                .UseMauiApp<App>()
#if ANDROID
                .UseGoogleMaps()
#elif IOS
                .UseGoogleMaps("AIzaSyCCbpV0ZR89ECwB6jakOR31PtZOpiUV5xA")
#endif

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // SQLite DB path
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "users.db3");
            builder.Services.AddSingleton(new UserDatabase(dbPath));

            var app = builder.Build();

            // Ensure navigation strings remain valid after renaming the page class.
            // Register both the old route name and the new class-based route.
            Routing.RegisterRoute("Profile", typeof(ProfilePage));
            Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
            // Register FriendsPage so Shell.Current.GoToAsync("FriendsPage") can resolve the route.
            Routing.RegisterRoute("FriendsPage", typeof(FriendsPage));
            // Register GroupsPage for group functionality
            Routing.RegisterRoute("GroupsPage", typeof(GroupsPage));
            // Register group chat page
            Routing.RegisterRoute("GroupChatPage", typeof(GroupChatPage));
            // Register edit group page
            Routing.RegisterRoute("EditGroupPage", typeof(EditGroupPage));

            return app;
        }
    }
}
