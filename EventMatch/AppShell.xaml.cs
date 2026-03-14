namespace EventMatch
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

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
