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

            // Always start with login page
            MyAppShell.CurrentItem = MyLoginPage;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Set("UserAlreadyLoggedIn", false);
            MyAppShell.CurrentItem = MyLoginPage;
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
