using System.Collections.Generic;
using KittyRef.Core.Referee;
using KittyRef.Core.Referee.Models;
using KittyRef.Core.Referee.Models.Events;

namespace KittyRef.Core.Models
{
    public class Player
    {
        /// <summary>
        /// The ID of the player.
        /// </summary>
        public int UserId { get; internal set; }

        /// <summary>
        /// The name of the player
        /// </summary>
        public string Username { get; internal set; } = string.Empty;

        /// <summary>
        /// The player's current status.
        /// </summary>
        /// <seealso cref="IRefereeHubClient.UserStatusChanged"/>
        public MatchUserStatus Status { get; internal set; }

        /// <summary>
        /// The player's current selected style.
        /// Only applicable if the current <see cref="PlaylistItem"/> enabled <see cref="PlaylistItem.Freestyle"/>.
        /// </summary>
        /// <seealso cref="IRefereeHubClient.UserStyleChanged"/>
        public Style Style { get; internal set; } = new Style();

        /// <summary>
        /// The player's current selected mods.
        /// Only applicable if the current <see cref="PlaylistItem"/> specifies any <see cref="PlaylistItem.AllowedMods"/>.
        /// </summary>
        /// <seealso cref="IRefereeHubClient.UserModsChanged"/>
        public List<Mod> UserMods { get; internal set; } = [];

        /// <summary>
        /// The player's current team.
        /// Only applicable if the room is in <see cref="MatchType.TeamVersus"/>.
        /// </summary>
        /// <seealso cref="IRefereeHubClient.UserTeamChanged"/>
        public MatchTeam? Team { get; internal set; }

        // public static Player From(Player player)
        // {
        //
        // }
    }
}
