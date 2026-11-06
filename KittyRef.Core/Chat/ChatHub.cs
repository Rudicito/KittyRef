using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using KittyRef.Core.Api;
using KittyRef.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using osu.NET.Internal;
using Websocket.Client;

namespace KittyRef.Core.Chat
{
    public class ChatHub: IDisposable
    {
        private readonly ITokenSource tokenSource;
        private readonly ILogger<ChatHub> logger;
        private volatile bool chatStarted;

        private readonly Uri url = new Uri("wss://notify.ppy.sh");
        private static readonly JsonSerializer serializer = OsuJsonSerializer.Create();

        private readonly WebsocketClient client;

        public event Action<ChatNewMessage>? NewMessage;

        public ConnectionState State => map(client);
        public event Action<ConnectionState>? ConnectionChanged;

        private static ConnectionState map(WebsocketClient client)
        {
            if (client.IsRunning) return ConnectionState.Connected;

            // We can consider it's connecting, because if it's not running, it tries indefinitely to reconnect
            else if (client.IsStarted)
                return ConnectionState.Connecting;

            return ConnectionState.Disconnected;
        }

        public ChatHub(ITokenSource tokenSource, ILoggerFactory? loggerFactory)
        {
            this.tokenSource = tokenSource;
            loggerFactory ??= new NullLoggerFactory();
            logger = loggerFactory.CreateLogger<ChatHub>();
            var webSocketLogger = loggerFactory.CreateLogger<WebsocketClient>();

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
                cli.Options.SetRequestHeader("Authorization", $"Bearer {tokenSource.GetAccessToken()}");
                return cli;
            });

            client = new WebsocketClient(url, webSocketLogger, factory)
            {
                Name = "osu-websocket",
                IsReconnectionEnabled = true,
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(10),
            };

            client.ReconnectionHappened.Subscribe(_ =>
            {
                ConnectionChanged.SafeInvoke(ConnectionState.Connected, nameof(ConnectionChanged), logger);
                if (chatStarted) StartChat();
            });

            client.DisconnectionHappened.Subscribe(_ =>
            {
                ConnectionChanged.SafeInvoke(ConnectionState.Disconnected, nameof(ConnectionChanged), logger);
            });

            client.MessageReceived.Subscribe(decodeMessage);

            tokenSource.TokenRefreshed += onTokenRefreshed;
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

                    NewMessage.SafeInvoke(payload, nameof(NewMessage), logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to decode websocket message");
            }
        }

        public Task StartAsync() => client.Start();

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

        private readonly SemaphoreSlim refreshLock = new(1, 1);

        private async void onTokenRefreshed(string _)
        {
            if (!client.IsStarted) return;

            if (!await refreshLock.WaitAsync(0)) return;

            try
            {
                await client.Reconnect();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reconnect after token refresh failed");
            }
            finally
            {
                refreshLock.Release();
            }
        }

        public void Dispose()
        {
            tokenSource.TokenRefreshed -= onTokenRefreshed;
            refreshLock.Dispose();
            client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
