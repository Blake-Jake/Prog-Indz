using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace EventMatch.Models
{
    public class DashboardPageView
    {
        public ICommand LogoutCommand { get; }

        public DashboardPageView()
        {
            LogoutCommand = new Command(PerformLogoutOperation);
        }

        private async void PerformLogoutOperation(object obj)
        {
            Preferences.Set("UserAlreadyLoggedIn", false);

            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
