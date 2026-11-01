using Microsoft.AspNetCore.SignalR.Client;
using osu.Server.Spectator.Hubs.Referee;
using TypedSignalR.Client;

namespace KittyRef.Core;

public class RefereeHub
{
    private const string hub_url = "https://spectator.ppy.sh/referee";
    private readonly HubConnection connection;

    /// <summary>
    /// The Hub use to call methods to the server.
    /// </summary>
    public readonly IRefereeHubServer Hub;

    public RefereeHub()
    {
        connection = new HubConnectionBuilder()
            .WithUrl(hub_url)
            .Build();

        Hub = connection.CreateHubProxy<IRefereeHubServer>();
    }
}
