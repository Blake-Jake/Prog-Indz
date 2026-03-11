namespace EventMatch
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Iš naujo pradžia - pirmas kart rodyti LoginPage
            var getuserSavedkey = Preferences.Get("UserAlreadyLoggedIn", false);

            if (getuserSavedkey)
            {
                MyAppShell.CurrentItem = MyDashboardPage;
            }
            else
            {
                MyAppShell.CurrentItem = MyLoginPage;
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Set("UserAlreadyLoggedIn", false);
            MyAppShell.CurrentItem = MyLoginPage;
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
