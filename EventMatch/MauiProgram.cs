using EventMatch.Services;
using System.IO;
using Microsoft.Maui.Controls;

namespace EventMatch
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
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

            return app;
        }
    }
}
