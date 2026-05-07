using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventMatch.Models;

namespace EventMatch.Services
{
    /// <summary>
    /// Demo scraper su fiktyviais renginiais testuoti
    /// </summary>
    public class DemoEventService
    {
        /// <summary>
        /// Grąžina demo renginius be Facebook API
        /// </summary>
        public async Task<List<Event>> GetDemoEventsAsync()
        {
            await Task.Delay(1000); // Simuliuoti delay

            var events = new List<Event>
            {
                new Event
                {
                    Details = "🎵 Metallica Koncertas - Vilnius Arena",
                    LocationAddress = "Vilnius Arena, Vilnius",
                    ScheduledAt = DateTime.Now.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    Tags = new List<Tag> 
                    { 
                        new Tag { Name = "Koncertas" },
                        new Tag { Name = "Muzika" },
                        new Tag { Name = "Demo" }
                    }
                },
                new Event
                {
                    Details = "⚽ Lietuvos futbolo čempionatas - Žalgiris vs Legia",
                    LocationAddress = "Lietuvos Nacionalinis Stadionas, Vilnius",
                    ScheduledAt = DateTime.Now.AddDays(3),
                    CreatedAt = DateTime.UtcNow,
                    Tags = new List<Tag> 
                    { 
                        new Tag { Name = "Sportas" },
                        new Tag { Name = "Futbolas" },
                        new Tag { Name = "Demo" }
                    }
                },
                new Event
                {
                    Details = "🎬 Lietuvos kino premjera - 'Žemyna'",
                    LocationAddress = "Cineplanet Akropolis, Vilnius",
                    ScheduledAt = DateTime.Now.AddDays(14),
                    CreatedAt = DateTime.UtcNow,
                    Tags = new List<Tag> 
                    { 
                        new Tag { Name = "Kinas" },
                        new Tag { Name = "Kultūra" },
                        new Tag { Name = "Demo" }
                    }
                },
                new Event
                {
                    Details = "🍕 Tarptautinis maisto festivalis - Street Food Festival",
                    LocationAddress = "Vilniaus Senamiestis, Gedimino pr.",
                    ScheduledAt = DateTime.Now.AddDays(10),
                    CreatedAt = DateTime.UtcNow,
                    Tags = new List<Tag> 
                    { 
                        new Tag { Name = "Festivalis" },
                        new Tag { Name = "Maistas" },
                        new Tag { Name = "Demo" }
                    }
                },
                new Event
                {
                    Details = "🎨 Ligoninės kryžiaus muziejus - 'Šiuolaikinis menas' paroda",
                    LocationAddress = "Ligoninės kryžiaus muziejus, Vilnius",
                    ScheduledAt = DateTime.Now.AddDays(21),
                    CreatedAt = DateTime.UtcNow,
                    Tags = new List<Tag> 
                    { 
                        new Tag { Name = "Menas" },
                        new Tag { Name = "Kultūra" },
                        new Tag { Name = "Demo" }
                    }
                }
            };

            return events;
        }
    }
}
