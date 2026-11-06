using System;
using System.Threading.Tasks;
using KittyRef.Core.Common;
using KittyRef.Core.Referee.Models;
using KittyRef.Core.Referee.Models.Events;
using Microsoft.Extensions.Logging;

namespace KittyRef.Core.Referee;

public class RefereeEvents(ILogger<RefereeHub> logger) : IRefereeHubClient
{
    /// <inheritdoc cref="IRefereeHubClient.UserJoined"/>
    public event Action<UserJoinedEvent>? UserJoined;

    /// <inheritdoc cref="IRefereeHubClient.UserLeft"/>
    public event Action<UserLeftEvent>? UserLeft;

    /// <inheritdoc cref="IRefereeHubClient.UserKicked"/>
    public event Action<UserKickedEvent>? UserKicked;

    /// <inheritdoc cref="IRefereeHubClient.UserBanned"/>
    public event Action<UserBannedEvent>? UserBanned;

    /// <inheritdoc cref="IRefereeHubClient.RefereeAdded"/>
    public event Action<RefereeAddedEvent>? RefereeAdded;

    /// <inheritdoc cref="IRefereeHubClient.RefereeRemoved"/>
    public event Action<RefereeRemovedEvent>? RefereeRemoved;

    /// <inheritdoc cref="IRefereeHubClient.RefereeInvited"/>
    public event Action<RefereeInvitedEvent>? RefereeInvited;

    /// <inheritdoc cref="IRefereeHubClient.RoomSettingsChanged"/>
    public event Action<RoomSettingsChangedEvent>? RoomSettingsChanged;

    /// <inheritdoc cref="IRefereeHubClient.MatchStateChanged"/>
    public event Action<MatchStateChangedEvent>? MatchStateChanged;

    /// <inheritdoc cref="IRefereeHubClient.PlaylistItemAdded"/>
    public event Action<PlaylistItemAddedEvent>? PlaylistItemAdded;

    /// <inheritdoc cref="IRefereeHubClient.PlaylistItemChanged"/>
    public event Action<PlaylistItemChangedEvent>? PlaylistItemChanged;

    /// <inheritdoc cref="IRefereeHubClient.PlaylistItemRemoved"/>
    public event Action<PlaylistItemRemovedEvent>? PlaylistItemRemoved;

    /// <inheritdoc cref="IRefereeHubClient.RollCompleted"/>
    public event Action<RollCompletedEvent>? RollCompleted;

    /// <inheritdoc cref="IRefereeHubClient.UserStatusChanged"/>
    public event Action<UserStatusChangedEvent>? UserStatusChanged;

    /// <inheritdoc cref="IRefereeHubClient.UserModsChanged"/>
    public event Action<UserModsChangedEvent>? UserModsChanged;

    /// <inheritdoc cref="IRefereeHubClient.UserStyleChanged"/>
    public event Action<UserStyleChangedEvent>? UserStyleChanged;

    /// <inheritdoc cref="IRefereeHubClient.UserTeamChanged"/>
    public event Action<UserTeamChangedEvent>? UserTeamChanged;

    /// <inheritdoc cref="IRefereeHubClient.CountdownStarted"/>
    public event Action<CountdownStartedEvent>? CountdownStarted;

    /// <inheritdoc cref="IRefereeHubClient.CountdownStopped"/>
    public event Action<CountdownStoppedEvent>? CountdownStopped;

    /// <inheritdoc cref="IRefereeHubClient.MatchStarted"/>
    public event Action<MatchStartedEvent>? MatchStarted;

    /// <inheritdoc cref="IRefereeHubClient.MatchAborted"/>
    public event Action<MatchAbortedEvent>? MatchAborted;

    /// <inheritdoc cref="IRefereeHubClient.MatchCompleted"/>
    public event Action<MatchCompletedEvent>? MatchCompleted;

    // obsolete, we don't care
    Task IRefereeHubClient.Pong(string message) => Task.CompletedTask;

    Task IRefereeHubClient.UserJoined(UserJoinedEvent info) => raise(UserJoined, info);
    Task IRefereeHubClient.UserLeft(UserLeftEvent info) => raise(UserLeft, info);
    Task IRefereeHubClient.UserKicked(UserKickedEvent info) => raise(UserKicked, info);
    Task IRefereeHubClient.UserBanned(UserBannedEvent info) => raise(UserBanned, info);
    Task IRefereeHubClient.RefereeAdded(RefereeAddedEvent info) => raise(RefereeAdded, info);
    Task IRefereeHubClient.RefereeRemoved(RefereeRemovedEvent info) => raise(RefereeRemoved, info);
    Task IRefereeHubClient.RefereeInvited(RefereeInvitedEvent info) => raise(RefereeInvited, info);
    Task IRefereeHubClient.RoomSettingsChanged(RoomSettingsChangedEvent info) => raise(RoomSettingsChanged, info);
    Task IRefereeHubClient.MatchStateChanged(MatchStateChangedEvent info) => raise(MatchStateChanged, info);
    Task IRefereeHubClient.PlaylistItemAdded(PlaylistItemAddedEvent info) => raise(PlaylistItemAdded, info);
    Task IRefereeHubClient.PlaylistItemChanged(PlaylistItemChangedEvent info) => raise(PlaylistItemChanged, info);
    Task IRefereeHubClient.PlaylistItemRemoved(PlaylistItemRemovedEvent info) => raise(PlaylistItemRemoved, info);
    Task IRefereeHubClient.RollCompleted(RollCompletedEvent info) => raise(RollCompleted, info);
    Task IRefereeHubClient.UserStatusChanged(UserStatusChangedEvent info) => raise(UserStatusChanged, info);
    Task IRefereeHubClient.UserModsChanged(UserModsChangedEvent info) => raise(UserModsChanged, info);
    Task IRefereeHubClient.UserStyleChanged(UserStyleChangedEvent info) => raise(UserStyleChanged, info);
    Task IRefereeHubClient.UserTeamChanged(UserTeamChangedEvent info) => raise(UserTeamChanged, info);
    Task IRefereeHubClient.CountdownStarted(CountdownStartedEvent info) => raise(CountdownStarted, info);
    Task IRefereeHubClient.CountdownStopped(CountdownStoppedEvent info) => raise(CountdownStopped, info);
    Task IRefereeHubClient.MatchStarted(MatchStartedEvent info) => raise(MatchStarted, info);
    Task IRefereeHubClient.MatchAborted(MatchAbortedEvent info) => raise(MatchAborted, info);
    Task IRefereeHubClient.MatchCompleted(MatchCompletedEvent info) => raise(MatchCompleted, info);

    private Task raise<T>(Action<T>? handler, T info)
    {
        handler.SafeInvoke(info, typeof(T).Name, logger);

        return Task.CompletedTask;
    }
}
