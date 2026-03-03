using EventMatch.Models;

namespace EventMatch;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardPageView();
    }
}