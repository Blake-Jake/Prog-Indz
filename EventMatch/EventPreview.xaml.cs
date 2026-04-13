using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using EventMatch.Services;
using EventMatch.Models;

namespace EventMatch;

public partial class EventPreview : ContentPage
{
	public EventPreview()
	{
		InitializeComponent();

	}

	private void OnRefreshClicked(object sender, EventArgs e)
	{
		// Reload events from the store and show a random un-favorited event (if any)
		var store = new EventStore();
		var events = store.LoadAll();
		var currentUser = Session.CurrentUserEmail;

		var newItems = events
			.Where(ev => ev.FavoritedBy == null || !ev.FavoritedBy.Contains(currentUser))
			.OrderByDescending(ev => ev.CreatedAt)
            .Select(e => new EventPreviewItem
            {
                Details = e.Details,
                CreatedAt = e.CreatedAt,
                LocationAddress = e.LocationAddress,  // add this
                ImageSource = string.IsNullOrEmpty(e.ImageBase64)
        ? ImageSource.FromFile("image-placeholder.png")
        : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(e.ImageBase64))),
                IsFavorite = e.FavoritedBy != null && e.FavoritedBy.Contains(currentUser)
            }).ToList();

		if (newItems != null && newItems.Count > 0)
		{
			_items = newItems;
			_currentIndex = newItems.Count > 1 ? Random.Shared.Next(newItems.Count) : 0;
			UpdateDisplayedEvent();
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		var store = new EventStore();
		var events = store.LoadAll();

		var currentUser = Session.CurrentUserEmail;

		// Exclude events already favorited by the current user
		_items = events
			.Where(e => e.FavoritedBy == null || !e.FavoritedBy.Contains(currentUser))
            .Select(e => new EventPreviewItem
            {
                Details = e.Details,
                CreatedAt = e.CreatedAt,
                LocationAddress = e.LocationAddress,  // add this
                ImageSource = string.IsNullOrEmpty(e.ImageBase64)
        ? ImageSource.FromFile("image-placeholder.png")
        : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(e.ImageBase64))),
                IsFavorite = e.FavoritedBy != null && e.FavoritedBy.Contains(currentUser)
            }).ToList();

		_currentIndex = 0;
     UpdateDisplayedEvent();

		// Ensure refresh button visibility matches whether there are events
		var refreshBtn = this.FindByName<Button>("RefreshButton");
		if (refreshBtn != null)
		{
			refreshBtn.IsVisible = (_items == null || _items.Count == 0);
		}
	}

   private List<EventPreviewItem> _items = new List<EventPreviewItem>();
	private int _currentIndex = 0;
	private EventStore _store = new EventStore();

  private void UpdateDisplayedEvent()
	{
        if (_items == null || _items.Count == 0)
		{
			// Show empty label and hide preview controls
			var empty = this.FindByName<Label>("NoEventsLabel");
			if (empty != null) empty.IsVisible = true;

			// Show message in the description box
			if (EventDetailsLabel != null)
			{
				EventDetailsLabel.IsVisible = true;
				EventDetailsLabel.Text = "No new events";
			}

			if (EventImage != null) EventImage.IsVisible = false;
			if (CreatedAtLabel != null) CreatedAtLabel.IsVisible = false;

            var locLabelEmpty = this.FindByName<Label>("LocationLabel");
            if (locLabelEmpty != null) locLabelEmpty.IsVisible = false;

            var refresh = this.FindByName<Button>("RefreshButton");
			if (refresh != null) refresh.IsVisible = true;

			return;
		}

		var item = _items[_currentIndex];

		// Update UI controls with the selected item
		if (EventDetailsLabel != null)
			EventDetailsLabel.Text = item.Details;

        var locLabel = this.FindByName<Label>("LocationLabel");
        if (locLabel != null)
        {
            if (!string.IsNullOrEmpty(item.LocationAddress))
            {
                locLabel.Text = $"📍 {item.LocationAddress}";
                locLabel.IsVisible = true;
            }
            else
            {
                locLabel.IsVisible = false;
            }
        }

        if (EventImage != null)
			EventImage.Source = item.ImageSource;

		if (CreatedAtLabel != null)
			CreatedAtLabel.Text = item.CreatedAt.ToString("g");

		// Update favorite button appearance
		var fav = this.FindByName<Button>("FavoriteButton");
		if (fav != null)
		{
			fav.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb(item.IsFavorite ? "#FF1493" : "#AA00AA00");
		}
	}

	private void OnCycleEventClicked(object sender, EventArgs e)
	{
		if (_items == null || _items.Count == 0)
			return;

		_currentIndex = (_currentIndex + 1) % _items.Count;
        UpdateDisplayedEvent();
	}

	private void OnFavoriteClicked(object sender, EventArgs e)
	{
		if (_items == null || _items.Count == 0)
			return;

		// Toggle favorite on the current event in the persistent store
      var stored = _store.LoadAll();
		var current = stored.FirstOrDefault(s => s.Details == _items[_currentIndex].Details && s.CreatedAt == _items[_currentIndex].CreatedAt);
		var user = Session.CurrentUserEmail;
		if (current != null)
		{
			if (current.FavoritedBy == null)
				current.FavoritedBy = new System.Collections.Generic.List<string>();

			if (current.FavoritedBy.Contains(user))
				current.FavoritedBy.Remove(user);
			else
				current.FavoritedBy.Add(user);

			_store.SaveAll(stored);

			// Update local cache: remove this item from the preview list if now favorited by this user
			if (current.FavoritedBy.Contains(user))
			{
				_items.RemoveAt(_currentIndex);
				if (_currentIndex >= _items.Count)
					_currentIndex = 0;
			}
		}

	// If there are no more items, show empty state and avoid modulo by zero
	if (_items == null || _items.Count == 0)
	{
		UpdateDisplayedEvent();
		return;
	}

	// After favoriting, move to the next event
	_currentIndex = (_currentIndex + 1) % _items.Count;
	UpdateDisplayedEvent();
	}
}

public class EventPreviewItem : INotifyPropertyChanged
{
	public string Details { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public ImageSource ImageSource { get; set; }
	public bool IsFavorite { get; set; }
    public string LocationAddress { get; set; } = string.Empty;

    private bool _isSelected;
	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
			}
		}
	}

	private bool _selectionEnabled;
	public bool SelectionEnabled
	{
		get => _selectionEnabled;
		set
		{
			if (_selectionEnabled != value)
			{
				_selectionEnabled = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionEnabled)));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
}
