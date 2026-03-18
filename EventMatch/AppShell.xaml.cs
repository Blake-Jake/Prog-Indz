namespace EventMatch
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for pages not defined as ShellContent
            Routing.RegisterRoute(nameof(EventPreview), typeof(EventPreview));

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
