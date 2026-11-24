using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittyRef.Core.Chat;
using KittyRef.Core.Models;
using KittyRef.Core.Models.Chat;
using KittyRef.Core.Referee;
using KittyRef.Core.Referee.Models;
using KittyRef.Core.Referee.Models.Events;
using KittyRef.Core.Referee.Models.Mapping;
using Microsoft.Extensions.Logging;
using osu.NET;

namespace KittyRef.Core;

public class RoomManager : IDisposable
{
    private readonly Action<Action> post;
    private readonly ILogger<RoomManager> logger;

    public IReadOnlyDictionary<long, Room> Rooms => rooms;
    private Dictionary<long, Room> rooms;

    public IReadOnlyDictionary<long, RoomInfo> ListRoom => listRoom;
    private Dictionary<long, RoomInfo> listRoom;

    private readonly RefereeHub refereeHub;
    private readonly ChatHub chatHub;
    private readonly OsuApiClient api;

    public RoomManager(RefereeHub refereeHub, ChatHub chatHub, OsuApiClient api, Action<Action> post, ILogger<RoomManager> logger)
    {
        this.refereeHub = refereeHub;
        this.chatHub = chatHub;
        this.api = api;
        this.post = post;
        this.logger = logger;
        rooms = new Dictionary<long, Room>();
        listRoom = new Dictionary<long, RoomInfo>();

        addEventRouter();
    }

    public async Task OpenRoom(RoomInfo roomInfo)
    {
        var response = await refereeHub.Hub.JoinRoom(roomInfo.RoomId);

        post(() =>
        {
            rooms.Add(response.RoomId, new Room(refereeHub.Hub, response));
        });
    }

    public async Task ReloadListRoom()
    {
        var response = await refereeHub.Hub.ListRooms();

        post(() =>
        {
            listRoom = response.RoomIDs
                .Select(id => new RoomInfo { RoomId = id })
                .ToDictionary(r => r.RoomId);
        });
    }

    #region EventHandler

    private void addEventRouter()
    {
        refereeHub.Events.UserJoined += onUserJoined;
        refereeHub.Events.UserLeft += onUserLeft;
        refereeHub.Events.UserKicked += onUserKicked;
        refereeHub.Events.UserBanned += onUserBanned;
        refereeHub.Events.RefereeAdded += onRefereeAdded;
        refereeHub.Events.RefereeRemoved += onRefereeRemoved;
        refereeHub.Events.RefereeInvited += onRefereeInvited;
        refereeHub.Events.RoomSettingsChanged += onRoomSettingsChanged;
        refereeHub.Events.MatchStateChanged += onMatchStateChanged;
        refereeHub.Events.PlaylistItemAdded += onPlaylistItemAdded;
        refereeHub.Events.PlaylistItemChanged += onPlaylistItemChanged;
        refereeHub.Events.PlaylistItemRemoved += onPlaylistItemRemoved;
        refereeHub.Events.RollCompleted += onRollCompleted;
        refereeHub.Events.UserStatusChanged += onUserStatusChanged;
        refereeHub.Events.UserModsChanged += onUserModsChanged;
        refereeHub.Events.UserStyleChanged += onUserStyleChanged;
        refereeHub.Events.UserTeamChanged += onUserTeamChanged;
        refereeHub.Events.CountdownStarted += onCountdownStarted;
        refereeHub.Events.CountdownStopped += onCountdownStopped;
        refereeHub.Events.MatchStarted += onMatchStarted;
        refereeHub.Events.MatchAborted += onMatchAborted;
        refereeHub.Events.MatchCompleted += onMatchCompleted;

        chatHub.NewMessage += onNewMessage;
    }

    private void removeEventRouter()
    {
        refereeHub.Events.UserJoined -= onUserJoined;
        refereeHub.Events.UserLeft -= onUserLeft;
        refereeHub.Events.UserKicked -= onUserKicked;
        refereeHub.Events.UserBanned -= onUserBanned;
        refereeHub.Events.RefereeAdded -= onRefereeAdded;
        refereeHub.Events.RefereeRemoved -= onRefereeRemoved;
        refereeHub.Events.RefereeInvited -= onRefereeInvited;
        refereeHub.Events.RoomSettingsChanged -= onRoomSettingsChanged;
        refereeHub.Events.MatchStateChanged -= onMatchStateChanged;
        refereeHub.Events.PlaylistItemAdded -= onPlaylistItemAdded;
        refereeHub.Events.PlaylistItemChanged -= onPlaylistItemChanged;
        refereeHub.Events.PlaylistItemRemoved -= onPlaylistItemRemoved;
        refereeHub.Events.RollCompleted -= onRollCompleted;
        refereeHub.Events.UserStatusChanged -= onUserStatusChanged;
        refereeHub.Events.UserModsChanged -= onUserModsChanged;
        refereeHub.Events.UserStyleChanged -= onUserStyleChanged;
        refereeHub.Events.UserTeamChanged -= onUserTeamChanged;
        refereeHub.Events.CountdownStarted -= onCountdownStarted;
        refereeHub.Events.CountdownStopped -= onCountdownStopped;
        refereeHub.Events.MatchStarted -= onMatchStarted;
        refereeHub.Events.MatchAborted -= onMatchAborted;
        refereeHub.Events.MatchCompleted -= onMatchCompleted;

        chatHub.NewMessage -= onNewMessage;
    }

    private void onUserJoined(UserJoinedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = new Player
            {
                UserId = (int)e.UserId
            };

            room.players.Add(player);

            _ = fetchUsernameAsync(player, e.RoomId, room.Lifetime);
        });
    }

    private void onUserLeft(UserLeftEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = room.GetPlayer(e.UserId);
            if (player != null) room.players.Remove(player);
        });
    }

    private void onUserKicked(UserKickedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = room.GetPlayer(e.KickedUserId);
            if (player != null) room.players.Remove(player);
        });
    }

    private void onUserBanned(UserBannedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var user = new User
            {
                UserId = (int)e.BannedUserId,
            };

            room.bannedUsers.Add(user);

            _ = fetchUsernameAsync(user, e.RoomId, room.Lifetime);
        });
    }

    private void onRefereeAdded(RefereeAddedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var user = new User
            {
                UserId = e.UserId
            };

            room.referees.Add(user);

            _ = fetchUsernameAsync(user, e.RoomId, room.Lifetime);
        });
    }

    private void onRefereeRemoved(RefereeRemovedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var referee = room.referees.FirstOrDefault(r => r.UserId == e.UserId);
            if (referee != null) room.referees.Remove(referee);
        });
    }

    private void onRefereeInvited(RefereeInvitedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onRoomSettingsChanged(RoomSettingsChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            room.Name = e.Name;
            room.Password = e.Password;
            room.Type = e.Type;
            room.QueueMode = e.QueueMode;
            room.MaxParticipants = e.MaxParticipants;
        });
    }

    private void onMatchStateChanged(MatchStateChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            room.Type = e.StateDto.Type;
            room.Locked = e.StateDto.Locked;
            room.Slots = e.StateDto.Slots?.ToArray();
        });
    }

    private void onPlaylistItemAdded(PlaylistItemAddedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            room.playlist.Add(e.PlaylistItemDto.FromDto());
        });
    }

    private void onPlaylistItemChanged(PlaylistItemChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var dto = e.PlaylistItemDto;
            var newPi = e.PlaylistItemDto.FromDto();
            var pi = room.GetPlaylistItem(dto.PlaylistItemId);
            if (pi == null) return;

            pi.Ruleset = newPi.Ruleset;
            pi.BeatmapId = newPi.BeatmapId;
            pi.requiredMods = newPi.requiredMods;
            pi.allowedMods = newPi.allowedMods;
            pi.Freestyle = newPi.Freestyle;
            pi.WasPlayed = newPi.WasPlayed;
            pi.Order = newPi.Order;
        });
    }

    private void onPlaylistItemRemoved(PlaylistItemRemovedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var playlistItem = room.GetPlaylistItem(e.PlaylistItemId);
            if (playlistItem != null) room.playlist.Remove(playlistItem);
        });
    }

    private void onRollCompleted(RollCompletedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onUserStatusChanged(UserStatusChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = room.GetPlayer(e.UserId);
            player?.Status = e.Status;
        });
    }

    private void onUserModsChanged(UserModsChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var ruleset = room.CurrentBeatmap?.Ruleset
                          ?? throw new Exception("User mods changed while Beatmap queue empty.");

            var player = room.GetPlayer(e.UserId);
            player?.UserMods = e.UserMods.Select(m => m.FromDto(ruleset)).ToList();
        });
    }

    private void onUserStyleChanged(UserStyleChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = room.GetPlayer(e.UserId);
            if (player == null) return;

            player.Ruleset = e.RulesetId == null ? null : RulesetMapping.ToRuleset(e.RulesetId.Value);
            player.BeatmapId = e.BeatmapId;
        });
    }

    private void onUserTeamChanged(UserTeamChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = room.GetPlayer(e.UserId);
            player?.Team = e.Team;
        });
    }

    private void onCountdownStarted(CountdownStartedEvent e)
    {
        var now = DateTimeOffset.UtcNow;

        updateRoom(e.RoomId, room =>
        {
            switch (e.Type)
            {
                case CountdownType.MatchStart:
                    room.MatchStartCountdown = new Countdown(e.CountdownId, e.Seconds, now);
                    break;

                case CountdownType.ServerShuttingDown:
                    room.ServerShuttingDownCountdown = new Countdown(e.CountdownId, e.Seconds, now);
                    break;

                default:
                    logger.LogWarning("Unknow countdown type received {Type} (ID: {CountdownId}) in room {RoomId}", e.Type, e.CountdownId, e.RoomId);
                    break;
            }
        });
    }

    private void onCountdownStopped(CountdownStoppedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            switch (e.Type)
            {
                case CountdownType.MatchStart:
                    if (room.MatchStartCountdown?.Id == e.CountdownId)
                    {
                        room.MatchStartCountdown = null;
                        room.NotifyChanged();
                    }
                    break;

                case CountdownType.ServerShuttingDown:
                    if (room.ServerShuttingDownCountdown?.Id == e.CountdownId)
                    {
                        room.ServerShuttingDownCountdown = null;
                        room.NotifyChanged();
                    }
                    break;

                default:
                    logger.LogWarning("Unknow countdown type received {Type} (ID: {CountdownId}) in room {RoomId}", e.Type, e.CountdownId, e.RoomId);
                    break;
            }
        }, false);
    }

    private void onMatchStarted(MatchStartedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onMatchAborted(MatchAbortedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onMatchCompleted(MatchCompletedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void updateRoom(long roomId, Action<Room> change, bool triggerChanged = true)
    {
        post(() =>
        {
            if (disposed)
                return;

            if (!rooms.TryGetValue(roomId, out var room))
                return;

            internalUpdateRoom(room, change, triggerChanged);
        });
    }

    private void internalUpdateRoom(Room room, Action<Room> change, bool triggerChanged = true)
    {
        try
        {
            change(room);

            if (triggerChanged) room.NotifyChanged();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to apply an event to room {RoomId}", room.RoomId);
        }
    }

    private void onNewMessage(ChatNewMessage e)
    {
        try
        {
            foreach (var group in e.Messages.GroupBy(m => m.ChannelId))
            {
                var messages = group.Select(m => new Message
                {
                    Id = m.MessageId,
                    Uuid = m.UUID ?? string.Empty,
                    UserId = m.SenderId,
                    Username = m.Sender?.Username,
                    Content = m.Content,
                    Timestamp = m.Timestamp
                }).ToList();

                long channelId = group.Key;

                post(() =>
                {
                    if (disposed) return;

                    var room = rooms.Values.FirstOrDefault(r => r.chatChannelId == channelId);
                    if (room == null) return;

                    internalUpdateRoom(room, r =>
                    {
                        r.addMessages(messages.ToArray());
                    });
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process a chat message event");
        }
    }

    private async Task fetchUsernameAsync(User user, long roomId, CancellationToken ct)
    {
        try
        {
            var response = await api.GetUserAsync(user.UserId, cancellationToken: ct);

            ct.ThrowIfCancellationRequested();

            if (response.IsFailure)
            {
                logger.LogWarning("An error occurred while fetching a username for the user {UserId} : {Error}", user.UserId, response.Error);
                return;
            }

            user.Username = response.Value?.Username;

            if (rooms.TryGetValue(roomId, out var room))
                room.NotifyChanged();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            logger.LogError(e, "An unexpected exception occurred while fetching a username of a user");
        }
    }

    #endregion

    private bool disposed;

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        foreach (var room in rooms)
            room.Value.Dispose();

        removeEventRouter();

        GC.SuppressFinalize(this);
    }
}
