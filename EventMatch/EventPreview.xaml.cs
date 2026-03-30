using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using EventMatch.Services;
using EventMatch.Models;

namespace EventMatch;

public partial class EventPreview : ContentPage
{
	public EventPreview()
	{
		InitializeComponent();

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
            ImageSource = string.IsNullOrEmpty(e.ImageBase64)
				? ImageSource.FromFile("image-placeholder.png")
				: ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(e.ImageBase64))),
			IsFavorite = e.FavoritedBy != null && e.FavoritedBy.Contains(currentUser)
        }).ToList();

		_currentIndex = 0;
		UpdateDisplayedEvent();
	}

   private List<EventPreviewItem> _items = new List<EventPreviewItem>();
	private int _currentIndex = 0;
	private EventStore _store = new EventStore();

  private void UpdateDisplayedEvent()
	{
        if (_items == null || _items.Count == 0)
			return;

		var item = _items[_currentIndex];

		// Update UI controls with the selected item
		if (EventDetailsLabel != null)
			EventDetailsLabel.Text = item.Details;

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

		// After favoriting, move to the next event
		_currentIndex = (_currentIndex + 1) % _items.Count;
		UpdateDisplayedEvent();
	}
}

public class EventPreviewItem
{
	public string Details { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public ImageSource ImageSource { get; set; }
    public bool IsFavorite { get; set; }
}
