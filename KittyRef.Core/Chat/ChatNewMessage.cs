using Newtonsoft.Json;
using osu.NET.Models.Chat;
using osu.NET.Models.Users;

namespace KittyRef.Core.Chat
{
    public class ChatNewMessage
    {
        [JsonProperty("messages")]
        public ChatMessage[] Messages { get; set; } = [];

        [JsonProperty("users")]
        public User[] Users { get; set; } = [];
    }
}
