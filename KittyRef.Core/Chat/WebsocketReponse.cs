using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KittyRef.Core.Chat
{
    [PublicAPI]
    public class WebsocketResponse
    {
        [JsonProperty("event")]
        public string Event { get; set; } = string.Empty;

        [JsonProperty("data")]
        public JToken? Data { get; set; }
    }
}
