using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KittyRef.Core.Models;
using KittyRef.Core.Referee;
using KittyRef.Core.Referee.Models.Events;
using RoomSettingsChangedEvent = KittyRef.Core.Referee.Models.Events.RoomSettingsChangedEvent;

namespace KittyRef.Core;

public class RoomManager
{
    public IReadOnlyDictionary<long, Room> Rooms => rooms;
    private Dictionary<long, Room> rooms;

    public IReadOnlyDictionary<long, RoomInfo> ListRoom => listRoom;
    private Dictionary<long, RoomInfo> listRoom;

    private readonly RefereeHub refereeHub;

    public RoomManager(RefereeHub refereeHub)
    {
        this.refereeHub = refereeHub;
        rooms = new Dictionary<long, Room>();
        listRoom = new Dictionary<long, RoomInfo>();
    }

    public async Task OpenRoom(RoomInfo roomInfo)
    {
        var response = await refereeHub.Hub.JoinRoom(roomInfo.RoomId);
        rooms.Add(response.RoomId, new Room(refereeHub.Hub, response));
    }

    public async Task ReloadListRoom()
    {
        var response = await refereeHub.Hub.ListRooms();

        listRoom = response.RoomIDs
            .Select(id => new RoomInfo { RoomId = id })
            .ToDictionary(r => r.RoomId);
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
            room.Players.Add(new Player
            {
                UserId = (int)e.UserId
            });
        });
    }

    private void onUserLeft(UserLeftEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = room.Players.FirstOrDefault(p => p.UserId == e.UserId);
            if (player != null) room.Players.Remove(player);
        });
    }

    private void onUserKicked(UserKickedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = room.Players.FirstOrDefault(p => p.UserId == e.KickedUserId);
            if (player != null) room.Players.Remove(player);
        });
    }

    private void onUserBanned(UserBannedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var player = room.Players.FirstOrDefault(p => p.UserId == e.BannedUserId);
            if (player != null) room.Players.Remove(player);
        });
    }

    private void onRefereeAdded(RefereeAddedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            room.Referees.Add(new Models.Referee
            {
                UserId = e.UserId
            });
        });
    }

    private void onRefereeRemoved(RefereeRemovedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
            var referee = room.Referees.FirstOrDefault(r => r.UserId == e.UserId);
            if (referee != null) room.Referees.Remove(referee);
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
        });
    }

    private void onMatchStateChanged(MatchStateChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onPlaylistItemAdded(PlaylistItemAddedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onPlaylistItemChanged(PlaylistItemChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onPlaylistItemRemoved(PlaylistItemRemovedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
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
        });
    }

    private void onUserModsChanged(UserModsChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onUserStyleChanged(UserStyleChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
        });
    }

    private void onUserTeamChanged(UserTeamChangedEvent e)
    {
        updateRoom(e.RoomId, room =>
        {
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

    private void updateRoom(long roomId, Action<Room> change)
    {
        if (!rooms.TryGetValue(roomId, out var room))
            return;

        change(room);
        room.NotifyChanged();
    }

    #endregion
}
