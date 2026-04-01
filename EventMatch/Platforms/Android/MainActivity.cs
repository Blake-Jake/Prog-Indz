using System;
using Android.App;
using Microsoft.Maui.Controls;
using Android.Content.PM;
using Android.OS;

namespace EventMatch
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            // Force cloud-only mode on Android to avoid using local DB/hybrid services
            System.Environment.SetEnvironmentVariable("EVENTMATCH_CLOUD_ONLY", "1");
            base.OnCreate(savedInstanceState);
        }

        public override void OnBackPressed()
        {
            // Allow Shell navigation to handle back button
            if (Shell.Current?.Navigation.NavigationStack.Count > 1)
            {
                Shell.Current.Navigation.PopAsync();
            }
            else
            {
                // If no pages to go back to, allow default behavior (exit app)
                base.OnBackPressed();
            }
        }
    }
}
