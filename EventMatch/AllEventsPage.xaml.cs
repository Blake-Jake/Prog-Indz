using Microsoft.Maui.Controls;
using EventMatch.Services;
using System.Linq;
using System.IO;
using EventMatch.Models;
using System.Collections.ObjectModel;

namespace EventMatch;

public partial class AllEventsPage : ContentPage
{
    // No manual fallback fields here; rely on XAML-generated members.
    public AllEventsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var store = new EventStore();
        var events = store.LoadAll();

        var items = events.Select(e => new EventPreviewItem
        {
            Details = e.Details,
            CreatedAt = e.CreatedAt,
            ImageSource = string.IsNullOrEmpty(e.ImageBase64)
                ? ImageSource.FromFile("image-placeholder.png")
                : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(e.ImageBase64)))
        }).ToList();

        // Only display favorites for the current user
        var currentUser = Session.CurrentUserEmail;
        var favorites = events
            .Where(e => e.FavoritedBy != null && e.FavoritedBy.Contains(currentUser))
            .Select(e => new EventPreviewItem
        {
            Details = e.Details,
            CreatedAt = e.CreatedAt,
            ImageSource = string.IsNullOrEmpty(e.ImageBase64)
                ? ImageSource.FromFile("image-placeholder.png")
                : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(e.ImageBase64)))
        }).ToList();

        AllEventsCollectionView.ItemsSource = new ObservableCollection<EventPreviewItem>(favorites);
    }
}
