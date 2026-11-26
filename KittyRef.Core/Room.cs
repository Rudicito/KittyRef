using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittyRef.Core.Models;
using KittyRef.Core.Models.Chat;
using KittyRef.Core.Referee;
using KittyRef.Core.Referee.Models;
using KittyRef.Core.Referee.Models.Mapping;
using KittyRef.Core.Referee.Models.Requests;
using KittyRef.Core.Referee.Models.Responses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace KittyRef.Core;

public class Room : IDisposable
{
    private readonly IRefereeHubServer hub;
    private readonly IApiRoom api;
    private readonly ILogger logger;

    private CancellationTokenSource cts;

    /// <summary>
    /// Cancelled when the room is disposed (left, closed or manager shut down).
    /// Should be used for background job.
    /// </summary>
    public CancellationToken Lifetime => cts.Token;

    /// <summary>
    /// The ID of the room.
    /// </summary>
    public long RoomId { get; internal set; }

    /// <summary>
    /// The chat channel of the room
    /// </summary>
    private readonly ChatChannel chatChannel;

    /// <summary>
    /// The ID of the chat channel of the room
    /// </summary>
    internal long chatChannelId => chatChannel.Id;

    /// <summary>
    /// The name of the room.
    /// </summary>
    public string Name { get; internal set; } = string.Empty;

    /// <summary>
    /// The password to the room.
    /// </summary>
    public string Password { get; internal set; } = string.Empty;

    /// <summary>
    /// The maximum number of players in the room.
    /// <list type="bullet">
    /// <item>If 0 or missing, the room will allow an unlimited number of participants, but will not have enabled player slots.</item>
    /// <item>If in the range [2, <see cref="MakeRoomRequest.MAX_PARTICIPANTS_LIMIT"/>] inclusive, the room will have the given number of slots to be occupied by participants.</item>
    /// </list>
    /// </summary>
    public int? MaxParticipants { get; internal set; }

    /// <summary>
    /// The current match type.
    /// </summary>
    public MatchType Type { get; internal set; }

    /// <summary>
    /// Whether the room is currently locked.
    /// When the room is locked, players cannot change teams themselves, only referees can change it for them.
    /// </summary>
    public bool Locked { get; internal set; }

    /// <summary>
    /// <para>
    /// The current state of slots in the room.
    /// </para>
    /// <para>
    /// If the whole array is <see langword="null"/>, then slots are not active in the room
    /// (there is no limit to number of participants).
    /// </para>
    /// <para>
    /// If the array is not <see langword="null"/>:
    /// <list type="bullet">
    /// <item>The length of the array is equal to the number of slots in the room.</item>
    /// <item>If a slot is empty, its corresponding entry in the array will be <see langword="null"/></item>.
    /// <item>If a slot is not empty, its corresponding entry in the array will be the ID of the user occupying it.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <seealso cref="MakeRoomRequest.MaxParticipants"/>
    /// <seealso cref="Core.Referee.Models.Events.RoomSettingsChangedEvent.MaxParticipants"/>
    public int?[]? Slots { get; internal set; }

    /// <summary>
    /// The current queue mode of the room.
    /// </summary>
    public QueueMode QueueMode { get; internal set; }

    public IReadOnlyList<PlaylistItem> Playlist => playlist;

    internal List<PlaylistItem> playlist = [];

    public IReadOnlyList<PlaylistItem> BeatmapQueue => Playlist.Where(i => !i.WasPlayed).OrderBy(i => i.Order).ToList();

    public PlaylistItem? CurrentBeatmap => BeatmapQueue.Count > 0 ? BeatmapQueue[0] : null;

    /// <summary>
    /// The list of players currently in the room.
    /// </summary>
    public IReadOnlyList<Player> Players => players;

    internal List<Player> players = [];

    /// <summary>
    /// The list of referees for the room.
    /// </summary>
    /// <remarks>
    /// This is not a list of connected referees: they are not necessarily in the room.
    /// </remarks>
    public IReadOnlyList<User> Referees => referees;

    internal List<User> referees = [];

    /// <summary>
    /// The list of banned users.
    /// </summary>
    public IReadOnlyList<User> BannedUsers => bannedUsers;

    internal List<User> bannedUsers = [];

    /// <summary>
    /// The countdown announcing the server shut down.
    /// </summary>
    public Countdown? ServerShuttingDownCountdown { get; internal set; }

    /// <summary>
    /// The countdown for the start of the match.
    /// </summary>
    public Countdown? MatchStartCountdown { get; internal set; }

    /// <summary>
    /// Raised whenever a property changes.
    /// </summary>
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();

    internal Room(IRefereeHubServer hub,  IApiRoom api, ILogger logger)
    {
        this.hub = hub;
        this.logger = logger;
        this.api = api;
        cts = new CancellationTokenSource();

        chatChannel = new ChatChannel(-1);
    }

    internal Room(IRefereeHubServer hub,  IApiRoom api, ILogger logger, RoomJoinedResponse response)
    {
        this.hub = hub;
        this.api = api;
        this.logger = logger;
        cts = new CancellationTokenSource();

        RoomId = response.RoomId;
        chatChannel = new ChatChannel(response.ChatChannelId);
        Name = response.Name;
        Password = response.Password;
        MaxParticipants = response.MaxParticipants;
        Type = response.StateDto.Type;
        Locked = response.StateDto.Locked;
        Slots = response.StateDto.Slots;
        QueueMode = response.QueueMode;
        referees = response.Referees.Select(r => new User{UserId = r.UserId}).ToList();

        playlist = response.Playlist.Select(PlaylistItemMapping.FromDto).ToList();

        var currentRuleset = Playlist.Where(pi => !pi.WasPlayed).MinBy(pi => pi.Order)?.Ruleset;

        players = response.Players.Select(m => m.FromDto(currentRuleset)).ToList();
    }

    #region Helper methods

    public PlaylistItem? GetPlaylistItem(long playlistItemId) => Playlist.FirstOrDefault(p => p.PlaylistItemId == playlistItemId);

    public Player? GetPlayer(long userId) => players.FirstOrDefault(p => p.UserId == userId);

    private async Task<bool> safeCall(Func<Task> action)
    {
        try
        {
            await action.Invoke();
            return true;
        }
        catch (OperationCanceledException) when (Lifetime.IsCancellationRequested)
        {
            return false;
        }
        catch (HubException e)
        {
            var refereeHubException = RefereeHubException.TryParse(e, logger);

            if (refereeHubException != null)
                logger.LogError(e, "Referee request refused by the server: Error {Code}: {Message}", refereeHubException.Code, refereeHubException.Message);
            else
                logger.LogError(e, "A request to the server failed");

            return false;
        }
        catch (Exception e)
        {
            logger.LogError(e, "A request to the server failed");
            return false;
        }
    }

    #endregion

    #region Message methods

    /// <summary>
    /// Send a message in the room.
    /// </summary>
    /// <param name="text">The message</param>
    public Task<bool> SendMessage(string text)
    {
        var message = new LocalMessage(text);
        formatMessage(message);
        chatChannel.addLocal(message);

        return safeCall(() => api.SendMessage(chatChannelId, message.Content, false, Lifetime));
    }

    internal void addMessages(Message[] messages)
    {
        foreach (var message in messages)
            formatMessage(message);

        chatChannel.addNewMessages(messages);
    }

    private void formatMessage(Message message)
    {
        try
        {
            MessageFormatter.FormatMessage(message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while formating a chat message");

            // fallback
            message.DisplayContent = message.Content;
        }
    }

    #endregion

    #region Referee methods

    /// <inheritdoc cref="IRefereeHubServer.CloseRoom"/>
    public Task<bool> CloseRoom()
    {
        return safeCall(() => hub.CloseRoom(RoomId));
    }

    /// <inheritdoc cref="IRefereeHubServer.InvitePlayer"/>
    public Task<bool> InvitePlayer(int userId)
    {
        return safeCall(() => hub.InvitePlayer(RoomId, userId));
    }

    /// <inheritdoc cref="IRefereeHubServer.KickPlayer"/>
    public Task<bool> KickPlayer(int userId)
    {
        return safeCall(() => hub.KickPlayer(RoomId, userId));
    }

    /// <inheritdoc cref="IRefereeHubServer.BanUser"/>
    public Task<bool> BanUser(int bannedUserId)
    {
        return safeCall(() => hub.BanUser(RoomId, bannedUserId));
    }

    /// <inheritdoc cref="IRefereeHubServer.AddReferee"/>
    public Task<bool> AddReferee(int targetUserId)
    {
        return safeCall(() => hub.AddReferee(RoomId, targetUserId));
    }

    /// <inheritdoc cref="IRefereeHubServer.RemoveReferee"/>
    public Task<bool> RemoveReferee(int targetUserId)
    {
        return safeCall(() => hub.RemoveReferee(RoomId, targetUserId));
    }

    /// <inheritdoc cref="IRefereeHubServer.ChangeRoomSettings"/>
    public Task<bool> ChangeRoomSettings(ChangeRoomSettingsRequest request)
    {
        return safeCall(() => hub.ChangeRoomSettings(RoomId, request));
    }

    /// <inheritdoc cref="IRefereeHubServer.EditCurrentPlaylistItem"/>
    public Task<bool> EditCurrentPlaylistItem(EditCurrentPlaylistItemRequest request)
    {
        return safeCall(() => hub.EditCurrentPlaylistItem(RoomId, request));
    }

    /// <inheritdoc cref="IRefereeHubServer.AddPlaylistItem"/>
    public Task<bool> AddPlaylistItem(AddPlaylistItemRequest request)
    {
        return safeCall(() => hub.AddPlaylistItem(RoomId, request));
    }

    /// <inheritdoc cref="IRefereeHubServer.EditPlaylistItem"/>
    public Task<bool> EditPlaylistItem(EditPlaylistItemRequest request)
    {
        return safeCall(() => hub.EditPlaylistItem(RoomId, request));
    }

    /// <inheritdoc cref="IRefereeHubServer.RemovePlaylistItem"/>
    public Task<bool> RemovePlaylistItem(RemovePlaylistItemRequest request)
    {
        return safeCall(() => hub.RemovePlaylistItem(RoomId, request));
    }

    /// <inheritdoc cref="IRefereeHubServer.Roll"/>
    public Task<bool> Roll(int max = 100)
    {
        Debug.Assert(max >= 2 && max <= 100);

        var request = new RollRequest
        {
            Max = (uint?)max
        };

        return safeCall(() => hub.Roll(RoomId, request));
    }

    /// <summary>
    /// Move a user
    /// </summary>
    public Task<bool> MoveUser(int userId, int slot)
    {
        var request = new MoveUserRequest
        {
            UserId = userId,
            Slot = (byte?)slot
        };

        return safeCall(() => hub.MoveUser(RoomId, request));
    }

    /// <summary>
    /// Change team of a player
    /// </summary>
    public Task<bool> ChangePlayerTeam(int userId, MatchTeam team)
    {
        var request = new MoveUserRequest
        {
            UserId = userId,
            Team = team
        };

        return safeCall(() => hub.MoveUser(RoomId, request));
    }

    /// <inheritdoc cref="IRefereeHubServer.SetLockState"/>
    public Task<bool> SetLockState(bool locked)
    {
        var request = new SetLockStateRequest
        {
            Locked = locked
        };

        return safeCall(() => hub.SetLockState(RoomId, request));
    }

    /// <inheritdoc cref="IRefereeHubServer.StartMatch"/>
    public Task<bool> StartMatch(int countdown = 0)
    {
        var request = new StartGameplayRequest
        {
            Countdown = countdown == 0 ? null : countdown
        };

        return safeCall(() => hub.StartMatch(RoomId, request));
    }

    /// <inheritdoc cref="IRefereeHubServer.StopMatchCountdown"/>
    public Task<bool> StopMatchCountdown()
    {
        return safeCall(() => hub.StopMatchCountdown(RoomId));
    }

    /// <inheritdoc cref="IRefereeHubServer.AbortMatch"/>
    public Task<bool> AbortMatch()
    {
        return safeCall(() => hub.AbortMatch(RoomId));
    }

    #endregion

    private bool disposed;

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        cts.Cancel();
        cts.Dispose();

        GC.SuppressFinalize(this);
    }
}
