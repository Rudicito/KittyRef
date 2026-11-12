using System;
using System.Threading;
using System.Threading.Tasks;
using KittyRef.Core.Api;
using KittyRef.Core.Common;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TypedSignalR.Client;

namespace KittyRef.Core.Referee;

public class RefereeHub: IAsyncDisposable
{
    private const string hub_url = "https://spectator.ppy.sh/referee";
    private readonly HubConnection connection;

    private volatile bool started;
    private volatile bool restarting;

    /// <summary>
    /// The Hub used to call methods to the server.
    /// </summary>
    public readonly IRefereeHubServer Hub;

    /// <summary>
    /// The events send by the server, used to subscribe to it.
    /// </summary>
    public readonly RefereeEvents Events;

    /// <summary>
    /// Raised after a reconnection.
    /// </summary>
    public event Action? Reconnected;

    public ConnectionState State => map(connection.State);
    public event Action<ConnectionState>? ConnectionChanged;

    private static ConnectionState map(HubConnectionState state) => state switch
    {
        HubConnectionState.Connected => ConnectionState.Connected,
        HubConnectionState.Disconnected => ConnectionState.Disconnected,
        _ => ConnectionState.Connecting
    };

    private readonly ITokenSource tokenSource;
    private readonly ILogger<RefereeHub> logger;
    private readonly CancellationTokenSource cts = new();
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    public RefereeHub(ITokenSource tokenSource, ILoggerFactory? loggerFactory = null)
    {
        this.tokenSource = tokenSource;
        loggerFactory ??= NullLoggerFactory.Instance;

        logger = loggerFactory.CreateLogger<RefereeHub>();

        var builder = new HubConnectionBuilder()
            .WithUrl(hub_url, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(tokenSource.GetAccessToken());
            })
            .WithAutomaticReconnect(new InfiniteRetryPolicy());

        // give to signalR the logger
        builder.Services.AddSingleton(loggerFactory);

        connection = builder.Build();

        connection.Reconnecting += _ =>
        {
            ConnectionChanged.SafeInvoke(ConnectionState.Connecting, nameof(ConnectionChanged), logger);
            return Task.CompletedTask;
        };

        connection.Reconnected += _ =>
        {
            ConnectionChanged.SafeInvoke(ConnectionState.Connected, nameof(ConnectionChanged), logger);
            Reconnected.SafeInvoke(nameof(Reconnected), logger);
            return Task.CompletedTask;
        };

        connection.Closed += _ =>
        {
            // StopAsync during a token restart also raises Closed: not a real disconnection
            if (!restarting)
                ConnectionChanged.SafeInvoke(ConnectionState.Disconnected, nameof(ConnectionChanged), logger);
            return Task.CompletedTask;
        };

        Hub = connection.CreateHubProxy<IRefereeHubServer>();

        Events = new RefereeEvents(logger);
        connection.Register<IRefereeHubClient>(Events);

        tokenSource.TokenRefreshed += onTokenRefreshed;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (started) return;

        ConnectionChanged.SafeInvoke(ConnectionState.Connecting, nameof(ConnectionChanged), logger);

        try
        {
            await connection.StartAsync(ct);
        }
        catch
        {
            ConnectionChanged.SafeInvoke(ConnectionState.Disconnected, nameof(ConnectionChanged), logger);
            throw;
        }

        started = true;
        ConnectionChanged.SafeInvoke(ConnectionState.Connected, nameof(ConnectionChanged), logger);
    }

    private async void onTokenRefreshed(string _)
    {
        if (!started || connection.State != HubConnectionState.Connected) return;

        if (!await refreshLock.WaitAsync(0)) return;

        try
        {
            restarting = true;
            ConnectionChanged.SafeInvoke(ConnectionState.Connecting, nameof(ConnectionChanged), logger);

            await connection.StopAsync(cts.Token);

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    await connection.StartAsync(cts.Token);
                    ConnectionChanged.SafeInvoke(ConnectionState.Connected, nameof(ConnectionChanged), logger);
                    Reconnected.SafeInvoke(nameof(Reconnected), logger);
                    return;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Referee hub restart failed, retrying in 10s");
                    await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while reconnecting to referee hub");
            ConnectionChanged.SafeInvoke(ConnectionState.Disconnected, nameof(ConnectionChanged), logger);
        }
        finally
        {
            restarting = false;
            refreshLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        started = false;
        tokenSource.TokenRefreshed -= onTokenRefreshed;
        await cts.CancelAsync();

        refreshLock.Dispose();
        await connection.DisposeAsync();
        cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
