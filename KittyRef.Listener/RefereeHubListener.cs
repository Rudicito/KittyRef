using System;
using System.Text.Json;
using System.Threading.Tasks;
using KittyRef.Core.Referee;

namespace KittyRef.Listener;

public class RefereeHubListener
{
    private readonly RefereeHub refereeHub;

    public RefereeHubListener(RefereeHub refereeHub)
    {
        this.refereeHub = refereeHub;
        var events = refereeHub.Events;

        events.UserJoined += e => print(nameof(events.UserJoined), e);
        events.UserLeft += e => print(nameof(events.UserLeft), e);
        events.UserKicked += e => print(nameof(events.UserKicked), e);
        events.UserBanned += e => print(nameof(events.UserBanned), e);
        events.RefereeAdded += e => print(nameof(events.RefereeAdded), e);
        events.RefereeRemoved += e => print(nameof(events.RefereeRemoved), e);
        events.RefereeInvited += e => print(nameof(events.RefereeInvited), e);
        events.RoomSettingsChanged += e => print(nameof(events.RoomSettingsChanged), e);
        events.MatchStateChanged += e => print(nameof(events.MatchStateChanged), e);
        events.PlaylistItemAdded += e => print(nameof(events.PlaylistItemAdded), e);
        events.PlaylistItemChanged += e => print(nameof(events.PlaylistItemChanged), e);
        events.PlaylistItemRemoved += e => print(nameof(events.PlaylistItemRemoved), e);
        events.RollCompleted += e => print(nameof(events.RollCompleted), e);
        events.UserStatusChanged += e => print(nameof(events.UserStatusChanged), e);
        events.UserModsChanged += e => print(nameof(events.UserModsChanged), e);
        events.UserStyleChanged += e => print(nameof(events.UserStyleChanged), e);
        events.UserTeamChanged += e => print(nameof(events.UserTeamChanged), e);
        events.CountdownStarted += e => print(nameof(events.CountdownStarted), e);
        events.CountdownStopped += e => print(nameof(events.CountdownStopped), e);
        events.MatchStarted += e => print(nameof(events.MatchStarted), e);
        events.MatchAborted += e => print(nameof(events.MatchAborted), e);
        events.MatchCompleted += e => print(nameof(events.MatchCompleted), e);
    }

    public Task StartAsync()
    {
        return refereeHub.StartAsync();
    }

    private static readonly JsonSerializerOptions json_options = new() { WriteIndented = false };
    private static void print(string name, object e)
    {
        Console.WriteLine($"{name}: {JsonSerializer.Serialize(e, json_options)}");
    }
}
