using EventMatch.Services;
using EventMatch.Models;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace EventMatch;

public partial class DataScraperPage : ContentPage
{
    public DataScraperPage()
    {
        InitializeComponent();
    }

    private async void OnScrapeDataClicked(object sender, EventArgs e)
    {
        var pageId = PageIdEntry?.Text?.Trim() ?? string.Empty;
        var accessToken = AccessTokenEntry?.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(pageId) || string.IsNullOrEmpty(accessToken))
        {
            ResultEditor.Text = "❌ Klaida: Prašome užpildyti Facebook Page ID ir Access Token";
            return;
        }

        try
        {
            StatusLabel.Text = "⏳ Skraipinama...";
            ResultEditor.Text = "Skraipinama Facebook renginiai...";

            var facebookService = new FacebookEventService(accessToken);
            var events = await facebookService.GetFacebookEventsAsync(pageId);

            if (events.Count == 0)
            {
                ResultEditor.Text = "❌ Nerasta renginių arba neteisinga Access Token / Page ID";
                StatusLabel.Text = "";
                return;
            }

            // Išsaugome renginius į EventStore
            var eventStore = new EventStore();
            foreach (var @event in events)
            {
                eventStore.Add(@event);
            }

            var resultText = $"✅ Sėkmingai importuota {events.Count} renginių!\n\n";
            resultText += "Renginiai:\n";
            resultText += new string('-', 50) + "\n";

            foreach (var @event in events)
            {
                resultText += $"📌 {(@event.Details ?? "Nėra pavadinimo")}\n";
                resultText += $"   📍 Vieta: {@event.LocationAddress}\n";
                resultText += $"   📅 Data: {@event.ScheduledAt:g}\n";
                resultText += $"   🏷️ Žymos: {string.Join(", ", @event.Tags.ConvertAll(t => t.Name))}\n";
                resultText += new string('-', 50) + "\n";
            }

            ResultEditor.Text = resultText;
            StatusLabel.Text = $"✅ {events.Count} renginių importuota!";

            // Galimybė grįžti į EventPreview
            await Task.Delay(2000);
            await Shell.Current.GoToAsync("EventPreview");
        }
        catch (Exception ex)
        {
            ResultEditor.Text = $"❌ Klaida: {ex.Message}\n\nPastaba: Patikrinkite, ar teisinga Access Token ir Page ID.";
            StatusLabel.Text = "Klaida";
        }
    }

    private async void OnLoadDemoClicked(object sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "⏳ Kraunami demo renginiai...";
            ResultEditor.Text = "Kraunami demo renginiai...";

            var demoService = new DemoEventService();
            var events = await demoService.GetDemoEventsAsync();

            if (events.Count == 0)
            {
                ResultEditor.Text = "❌ Nepavyko gauti demo renginių";
                StatusLabel.Text = "";
                return;
            }

            // Išsaugome renginius į EventStore
            var eventStore = new EventStore();
            foreach (var @event in events)
            {
                eventStore.Add(@event);
            }

            var resultText = $"✅ Sėkmingai importuota {events.Count} demo renginių!\n\n";
            resultText += "Renginiai:\n";
            resultText += new string('-', 50) + "\n";

            foreach (var @event in events)
            {
                resultText += $"📌 {(@event.Details ?? "Nėra pavadinimo")}\n";
                resultText += $"   📍 Vieta: {@event.LocationAddress}\n";
                resultText += $"   📅 Data: {@event.ScheduledAt:g}\n";
                resultText += $"   🏷️ Žymos: {string.Join(", ", @event.Tags.ConvertAll(t => t.Name))}\n";
                resultText += new string('-', 50) + "\n";
            }

            ResultEditor.Text = resultText;
            StatusLabel.Text = $"✅ {events.Count} demo renginių importuota!";

            // Galimybė grįžti į EventPreview
            await Task.Delay(2000);
            await Shell.Current.GoToAsync("EventPreview");
        }
        catch (Exception ex)
        {
            ResultEditor.Text = $"❌ Klaida: {ex.Message}";
            StatusLabel.Text = "Klaida";
        }
    }
}
