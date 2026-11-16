using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KittyRef.Core.Models;
using KittyRef.Core.Referee;
using KittyRef.Core.Referee.Models.Events;
using KittyRef.Core.Referee.Models.Mapping;
using Player = KittyRef.Core.Models.Player;

namespace KittyRef.Core;

public class RoomManager
{
    private readonly Action<Action> post;

    public IReadOnlyDictionary<long, Room> Rooms => rooms;
    private Dictionary<long, Room> rooms;

    public IReadOnlyDictionary<long, RoomInfo> ListRoom => listRoom;
    private Dictionary<long, RoomInfo> listRoom;

    private readonly RefereeHub refereeHub;

    public RoomManager(RefereeHub refereeHub, Action<Action> post)
    {
        this.post = post;
        this.refereeHub = refereeHub;
        rooms = new Dictionary<long, Room>();
        listRoom = new Dictionary<long, RoomInfo>();
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
    }

    private void onUserJoined(UserJoinedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            room.players.Add(new Player
            {
                UserId = (int)e.UserId
            });
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
            room.bannedUsers.Add(new User
            {
                UserId = (int)e.BannedUserId
                //todo: add username
            });
        });
    }

    private void onRefereeAdded(RefereeAddedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            room.referees.Add(new User
            {
                UserId = e.UserId
                //todo: add username
            });
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
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onCountdownStopped(CountdownStoppedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
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
            if (!rooms.TryGetValue(roomId, out var room))
                return;

            change(room);

            if (triggerChanged) room.NotifyChanged();
        });
    }

    #endregion
}
