using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittyRef.Core.Models;
using KittyRef.Core.Referee;
using KittyRef.Core.Referee.Models;
using KittyRef.Core.Referee.Models.Mapping;
using KittyRef.Core.Referee.Models.Requests;
using KittyRef.Core.Referee.Models.Responses;

namespace KittyRef.Core;

public class Room : IDisposable
{
    private readonly IRefereeHubServer hub;

    private CancellationTokenSource cts = new CancellationTokenSource();

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
    /// The ID of the chat channel for the joined multiplayer room.
    /// </summary>
    public long ChatChannelId { get; internal set; }

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

    public IReadOnlyList<Player> Players => players;

    internal List<Player> players = [];

    public IReadOnlyList<User> Referees => referees;

    internal List<User> referees = [];

    public IReadOnlyList<User> BannedUsers => bannedUsers;

    internal List<User> bannedUsers = [];

    public IReadOnlyList<Countdown> Countdowns => countdowns;

    internal List<Countdown> countdowns = [];

    public IReadOnlyList<ChatMessage> ChatMessages => chatMessages;

    internal List<ChatMessage> chatMessages = [];

    /// <summary>
    /// Trigger at each property modification
    /// </summary>
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();

    public Room(IRefereeHubServer hub)
    {
        this.hub = hub;
    }

    public Room(IRefereeHubServer hub, RoomJoinedResponse response)
    {
        this.hub = hub;

        RoomId = response.RoomId;
        ChatChannelId = response.ChatChannelId;
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

    #endregion

    #region Referee methods

    /// <inheritdoc cref="IRefereeHubServer.CloseRoom"/>
    public Task CloseRoom()
    {
        return hub.CloseRoom(RoomId);
    }

    /// <inheritdoc cref="IRefereeHubServer.InvitePlayer"/>
    public Task InvitePlayer(int userId)
    {
        return hub.InvitePlayer(RoomId, userId);
    }

    /// <inheritdoc cref="IRefereeHubServer.KickPlayer"/>
    public Task KickPlayer(int userId)
    {
        return hub.KickPlayer(RoomId, userId);
    }

    /// <inheritdoc cref="IRefereeHubServer.BanUser"/>
    public Task BanUser(int bannedUserId)
    {
        return hub.BanUser(RoomId, bannedUserId);
    }

    /// <inheritdoc cref="IRefereeHubServer.AddReferee"/>
    public Task AddReferee(int targetUserId)
    {
        return hub.AddReferee(RoomId, targetUserId);
    }

    /// <inheritdoc cref="IRefereeHubServer.RemoveReferee"/>
    public Task RemoveReferee(int targetUserId)
    {
        return hub.RemoveReferee(RoomId, targetUserId);
    }

    /// <inheritdoc cref="IRefereeHubServer.ChangeRoomSettings"/>
    public Task ChangeRoomSettings(ChangeRoomSettingsRequest request)
    {
        return hub.ChangeRoomSettings(RoomId, request);
    }

    /// <inheritdoc cref="IRefereeHubServer.EditCurrentPlaylistItem"/>
    public Task EditCurrentPlaylistItem(EditCurrentPlaylistItemRequest request)
    {
        return hub.EditCurrentPlaylistItem(RoomId, request);
    }

    /// <inheritdoc cref="IRefereeHubServer.AddPlaylistItem"/>
    public Task AddPlaylistItem(AddPlaylistItemRequest request)
    {
        return hub.AddPlaylistItem(RoomId, request);
    }

    /// <inheritdoc cref="IRefereeHubServer.EditPlaylistItem"/>
    public Task EditPlaylistItem(EditPlaylistItemRequest request)
    {
        return hub.EditPlaylistItem(RoomId, request);
    }

    /// <inheritdoc cref="IRefereeHubServer.RemovePlaylistItem"/>
    public Task RemovePlaylistItem(RemovePlaylistItemRequest request)
    {
        return hub.RemovePlaylistItem(RoomId, request);
    }

    /// <inheritdoc cref="IRefereeHubServer.Roll"/>
    public Task Roll(int max = 100)
    {
        Debug.Assert(max >= 2 && max <= 100);

        var request = new RollRequest
        {
            Max = (uint?)max
        };

        return hub.Roll(RoomId, request);
    }

    /// <summary>
    /// Move a user
    /// </summary>
    public Task MoveUser(int userId, int slot)
    {
        var request = new MoveUserRequest
        {
            UserId = userId,
            Slot = (byte?)slot
        };

        return hub.MoveUser(RoomId, request);
    }

    /// <summary>
    /// Change team to a player
    /// </summary>
    public Task ChangeTeam(int userId, MatchTeam team)
    {
        var request = new MoveUserRequest
        {
            UserId = userId,
            Team = team
        };

        return hub.MoveUser(RoomId, request);
    }

    /// <inheritdoc cref="IRefereeHubServer.SetLockState"/>
    public Task SetLockState(bool locked)
    {
        var request = new SetLockStateRequest
        {
            Locked = locked
        };

        return hub.SetLockState(RoomId, request);
    }

    /// <inheritdoc cref="IRefereeHubServer.StartMatch"/>
    public Task StartMatch(int countdown)
    {
        var request = new StartGameplayRequest
        {
            Countdown = countdown
        };

        return hub.StartMatch(RoomId, request);
    }

    /// <inheritdoc cref="IRefereeHubServer.StopMatchCountdown"/>
    public Task StopMatchCountdown()
    {
        return hub.StopMatchCountdown(RoomId);
    }

    /// <inheritdoc cref="IRefereeHubServer.AbortMatch"/>
    public Task AbortMatch()
    {
        return hub.AbortMatch(RoomId);
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
