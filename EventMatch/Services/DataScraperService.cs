using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EventMatch.Models;

namespace EventMatch.Services
{
    public class DataScraperService
    {
        private readonly HttpClient _httpClient;

        public DataScraperService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<ScrapedEvent>> ScrapeEventsAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                // Placeholder: return empty list
                return new List<ScrapedEvent>();
            }
            catch (Exception ex)
            {
                return new List<ScrapedEvent>();
            }
        }
    }

    public class ScrapedEvent
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Url { get; set; }
    }

    /// <summary>
    /// Facebook Events scraper using Graph API
    /// </summary>
    public class FacebookEventService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private const string GraphApiUrl = "https://graph.facebook.com/v19.0";

        public FacebookEventService(string accessToken)
        {
            _httpClient = new HttpClient();
            _accessToken = accessToken;
        }

        /// <summary>
        /// Gauna renginius iš Facebook puslapio
        /// </summary>
        /// <param name="pageId">Facebook puslapio ID</param>
        /// <returns>EventMatch Event objektų sąrašas</returns>
        public async Task<List<Event>> GetFacebookEventsAsync(string pageId)
        {
            try
            {
                // Facebook Graph API query
                var url = $"{GraphApiUrl}/{pageId}/events?fields=id,name,description,start_time,end_time,place,cover&access_token={_accessToken}";
                
                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);
                var events = new List<Event>();

                if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement))
                {
                    foreach (var eventElement in dataElement.EnumerateArray())
                    {
                        var @event = ParseFacebookEvent(eventElement);
                        if (@event != null)
                        {
                            events.Add(@event);
                        }
                    }
                }

                return events;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facebook scraping error: {ex.Message}");
                return new List<Event>();
            }
        }

        /// <summary>
        /// Konvertuoja Facebook event JSON į Event modelį
        /// </summary>
        private Event ParseFacebookEvent(JsonElement fbEvent)
        {
            try
            {
                var @event = new Event
                {
                    Details = fbEvent.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : string.Empty,
                    LocationAddress = fbEvent.TryGetProperty("place", out var place) ? ParsePlace(place) : string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    Tags = new List<Tag> { new Tag { Name = "Facebook" } }
                };

                // Parsavimas pradžios laiko
                if (fbEvent.TryGetProperty("start_time", out var startTime))
                {
                    if (DateTime.TryParse(startTime.GetString(), out var scheduledTime))
                    {
                        @event.ScheduledAt = scheduledTime;
                    }
                }

                // Parsavimas vaizdo
                if (fbEvent.TryGetProperty("cover", out var cover) && cover.TryGetProperty("source", out var source))
                {
                    @event.ImageBase64 = DownloadImageAsBase64(source.GetString()).Result ?? string.Empty;
                }

                return @event;
            }
            catch
            {
                return null;
            }
        }

        private string ParsePlace(JsonElement place)
        {
            try
            {
                if (place.TryGetProperty("name", out var name))
                    return name.GetString() ?? "Unknown location";
            }
            catch { }
            
            return "Unknown location";
        }

        /// <summary>
        /// Atsisiunčia vaizdą ir konvertuoja į Base64
        /// </summary>
        private async Task<string> DownloadImageAsBase64(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return string.Empty;

                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                return Convert.ToBase64String(imageBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
