using System;
using System.Collections.Generic;
using System.Text;

namespace EventMatch.Models
{
    public static class Session
    {
        // Static field to hold the current user's email
        private static string _currentUserEmail = string.Empty;
        private const string CURRENT_USER_EMAIL_KEY = "CurrentUserEmail";

        public static string CurrentUserEmail
        {
            get => _currentUserEmail;
            set
            {
                _currentUserEmail = value;
                // Also persist to Preferences for app restart recovery
                if (!string.IsNullOrEmpty(value))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Preferences.Set(CURRENT_USER_EMAIL_KEY, value);
                        System.Diagnostics.Debug.WriteLine($"[Session] Saved email to Preferences: {value}");
                    });
                }
            }
        }

        /// <summary>
        /// Restore session from Preferences (call this on app startup)
        /// </summary>
        public static void RestoreFromPreferences()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _currentUserEmail = Preferences.Get(CURRENT_USER_EMAIL_KEY, string.Empty);
                System.Diagnostics.Debug.WriteLine($"[Session] Restored email from Preferences: '{_currentUserEmail}'");
            });
        }

        /// <summary>
        /// Clear session (logout)
        /// </summary>
        public static void Clear()
        {
            _currentUserEmail = string.Empty;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Preferences.Remove(CURRENT_USER_EMAIL_KEY);
                System.Diagnostics.Debug.WriteLine("[Session] Session cleared");
            });
        }
    }
}
