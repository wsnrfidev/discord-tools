using System;
using System.Collections.Generic;

namespace DiscordTools
{
    public class EmbedMessage
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public string Color { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public List<EmbedFields> Fields { get; set; } = new List<EmbedFields>();
        public string FooterText { get; set; }
        public string FooterIcon { get; set; }
        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;

    }
}
