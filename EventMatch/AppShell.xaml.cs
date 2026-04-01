using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using EventMatch.Services;
using EventMatch.Models;

namespace EventMatch
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for pages not defined as ShellContent
            Routing.RegisterRoute(nameof(EventPreview), typeof(EventPreview));
            Routing.RegisterRoute(nameof(EventCreator), typeof(EventCreator));
            Routing.RegisterRoute(nameof(AllEventsPage), typeof(AllEventsPage));
            Routing.RegisterRoute(nameof(EditGroupPage), typeof(EditGroupPage));
            Routing.RegisterRoute(nameof(GroupChatPage), typeof(GroupChatPage));

            // Restore user session from preferences
            Session.RestoreFromPreferences();
            System.Diagnostics.Debug.WriteLine($"[AppShell] After restore, Session.CurrentUserEmail = '{Session.CurrentUserEmail}'");

            // Try update header from restored session (non-blocking)
            if (!string.IsNullOrEmpty(Session.CurrentUserEmail))
            {
                _ = UpdateUserHeaderAsync(Session.CurrentUserEmail);
            }

            // Initialize hybrid services for cloud sync
            _ = InitializeHybridServicesAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Now Shell.Current is available
            bool isLoggedIn = Preferences.Get("UserAlreadyLoggedIn", false);
            if (!isLoggedIn)
            {
                CurrentItem = this.FindByName<ShellContent>("MyLoginPage");
                _ = Shell.Current.GoToAsync("//LoginPage");
            }
        }

        /// <summary>
        /// Update the flyout header labels with the given user email and profile username (if any).
        /// Call this after successful login to refresh the UI.
        /// </summary>
        public async Task UpdateUserHeaderAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email)) return;

                // Update email label on UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var emailLabel = this.FindByName<Label>("UserEmailLabel");
                    if (emailLabel != null) emailLabel.Text = email;
                });

                // Load profile username from DB
                var userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<UserDatabase>();
                if (userDb != null)
                {
                    var profile = await userDb.GetProfileByEmailAsync(email);
                    if (profile != null && !string.IsNullOrEmpty(profile.Username))
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            var nameLabel = this.FindByName<Label>("UserNameLabel");
                            if (nameLabel != null) nameLabel.Text = profile.Username;
                        });
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Initialize hybrid services to enable cloud synchronization
        /// </summary>
        private async Task InitializeHybridServicesAsync()
        {
            try
            {
                var hybridAuth = Application.Current?.Handler?.MauiContext?.Services.GetService<HybridAuthService>();
                if (hybridAuth != null)
                {
                    await hybridAuth.InitializeAsync();
                }

                var hybridGroup = Application.Current?.Handler?.MauiContext?.Services.GetService<HybridGroupService>();
                if (hybridGroup != null)
                {
                    await hybridGroup.InitializeAsync();
                    // Attempt to sync local data to cloud on startup
                    await hybridGroup.SyncLocalToCloudAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hybrid services initialization error: {ex.Message}");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Set("UserAlreadyLoggedIn", false);
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
