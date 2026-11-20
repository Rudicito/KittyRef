using System;

namespace KittyRef.Core.Models
{
    public class ChatMessage
    {
        public required int UserId { get; set; }
        public required string? Username { get; set; }
        public required string Message { get; set; }
        public required DateTimeOffset Timestamp { get; set; }
    }
}
