using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace EventMatch;

public partial class EventPreview : ContentPage
{
    // (Use generated members from XAML source generator)
	public EventPreview()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Load last saved event from preferences (simple persistence)
		var details = Preferences.Get("LastEvent_Details", string.Empty);
		var imageBase64 = Preferences.Get("LastEvent_ImageBase64", string.Empty);

		if (!string.IsNullOrEmpty(details))
		{
			if (EventDetailsLabel != null)
				EventDetailsLabel.Text = details;
		}

		if (!string.IsNullOrEmpty(imageBase64))
		{
			try
			{
				var bytes = Convert.FromBase64String(imageBase64);
				if (EventImage != null)
					EventImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
			}
			catch { }
		}
	}
}
