using System;

namespace EventMatch.Models
{
    public class Event
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Details { get; set; } = string.Empty;
        public string ImageBase64 { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationAddress { get; set; } = string.Empty;
        // List of user emails who favorited this event
        public System.Collections.Generic.List<string> FavoritedBy { get; set; } = new System.Collections.Generic.List<string>();
    }
}
