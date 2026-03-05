using EventMatch.Models;

namespace EventMatch;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardPageView();
    }

    private async void OnOpenProfileClicked(object? sender, EventArgs e)
    {

        await Shell.Current.GoToAsync("ProfilePage");
    }
}