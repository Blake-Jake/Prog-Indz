namespace EventMatch
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            var getuserSavedkey = Preferences.Get("UserAlreadyLoggedIn", false);

            if (getuserSavedkey == true)
            {
                MyAppShell.CurrentItem = MyDashboardPage;
            }
            else
            {
                MyAppShell.CurrentItem = MyLoginPage;
            }
        }
    }
}
