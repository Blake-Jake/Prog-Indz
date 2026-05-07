using EventMatch.Models;
using EventMatch.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventMatch.Tests
{
    public class TagsAndScraperTests
    {
        public static void TestTagsCreation()
        {
            // Test 1: Create tags
            var tags = new List<Tag>
            {
                new Tag { Name = "sportas" },
                new Tag { Name = "koncertas" },
                new Tag { Name = "šeima" }
            };

            Console.WriteLine("✓ Tags created successfully");
            foreach (var tag in tags)
            {
                Console.WriteLine($"  - {tag.Name}");
            }
        }

        public static void TestEventWithTags()
        {
            // Test 2: Create event with tags
            var @event = new Event
            {
                Id = Guid.NewGuid().ToString(),
                Details = "Test event",
                Tags = new List<Tag>
                {
                    new Tag { Name = "sportas" },
                    new Tag { Name = "smagus" }
                }
            };

            Console.WriteLine($"✓ Event created with {(@event.Tags?.Count ?? 0)} tags");
            if (@event.Tags != null)
            {
                foreach (var tag in @event.Tags)
                {
                    Console.WriteLine($"  - {tag.Name}");
                }
            }
        }

        public static async Task TestDataScraper()
        {
            // Test 3: Test DataScraperService
            var scraper = new DataScraperService();
            var result = await scraper.ScrapeEventsAsync("https://jsonplaceholder.typicode.com/posts/1");
            Console.WriteLine($"✓ DataScraperService executed: found {result.Count} events");
        }
    }
}
