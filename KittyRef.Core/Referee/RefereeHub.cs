using System;
using System.Threading;
using System.Threading.Tasks;
using KittyRef.Core.Referee.Models;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;

namespace KittyRef.Core.Referee;

public class RefereeHub: IAsyncDisposable
{
    private const string hub_url = "https://spectator.ppy.sh/referee";
    private readonly HubConnection connection;

    /// <summary>
    /// The Hub used to call methods to the server.
    /// </summary>
    public readonly IRefereeHubServer Hub;

    /// <summary>
    /// The events send by the server, used to subscribe to it.
    /// </summary>
    public readonly RefereeEvents Events;

    public RefereeHub(string accessToken)
    {
        connection = new HubConnectionBuilder()
            .WithUrl(hub_url, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
            })
            .WithAutomaticReconnect()
            .Build();

        Hub = connection.CreateHubProxy<IRefereeHubServer>();

        Events = new RefereeEvents();
        connection.Register<IRefereeHubClient>(Events);
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        return connection.StartAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

}
