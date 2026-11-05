using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using osu.NET.Internal;
using Websocket.Client;

namespace KittyRef.Core.Chat
{
    public class ChatHub: IDisposable
    {
        private volatile string token;
        private readonly ILogger<ChatHub> logger;
        private volatile bool chatStarted;

        private readonly Uri url = new Uri("wss://notify.ppy.sh");
        private static readonly JsonSerializer serializer = OsuJsonSerializer.Create();

        private readonly WebsocketClient client;

        public event Action<ChatNewMessage>? NewMessage;

        public ChatHub(string token, ILogger<ChatHub>? logger)
        {
            this.token = token;
            this.logger = logger ?? new NullLogger<ChatHub>();

            var factory = new Func<ClientWebSocket>(() =>
            {
                var cli = new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(20),
                        KeepAliveTimeout = TimeSpan.FromSeconds(10)
                    }
                };
                cli.Options.SetRequestHeader("Authorization", $"Bearer {this.token}");
                return cli;
            });

            client = new WebsocketClient(url, factory)
            {
                IsReconnectionEnabled = true,
                ReconnectTimeout = null,
            };

            client.ReconnectionHappened.Subscribe(_ =>
            {
                if (chatStarted) StartChat();
            });


            client.MessageReceived.Subscribe(decodeMessage);
        }

        private void decodeMessage(ResponseMessage msg)
        {
            if (msg.MessageType != WebSocketMessageType.Text || msg.Text == null) return;

            try
            {
                using var reader = new JsonTextReader(new StringReader(msg.Text));
                var response = serializer.Deserialize<WebsocketResponse>(reader);
                if (response == null) return;

                if (response.Event == "chat.message.new")
                {
                    var payload = response.Data?.ToObject<ChatNewMessage>(serializer);
                    if (payload is null) return;

                    NewMessage?.Invoke(payload);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to decode websocket message");
            }
        }

        public Task StartAsync() => client.StartOrFail();

        public bool StartChat()
        {
            chatStarted = true;
            return client.Send("{\"event\": \"chat.start\"}");
        }

        public bool StopChat()
        {
            chatStarted = false;
            return client.Send("{\"event\": \"chat.end\"}");
        }

        public Task UpdateToken(string newToken)
        {
            if (newToken == token) return Task.CompletedTask;
            token = newToken;
            return client.Reconnect();
        }

        public void Dispose()
        {
            client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
