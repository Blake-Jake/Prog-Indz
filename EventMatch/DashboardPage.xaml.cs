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

    private async void OnMapsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("EventMap");
    }

    private async void OnStartLookingClicked(object? sender, EventArgs e)
    {
        // Navigate to the event preview page
        await Shell.Current.GoToAsync("EventPreview");
    }

    private async void OnCreateEventClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("EventCreator");
    }
}