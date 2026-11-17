using System.Collections.Generic;
using KittyRef.Core.Models.Mods;
using KittyRef.Core.Referee;
using KittyRef.Core.Referee.Models;

namespace KittyRef.Core.Models
{
    public class Player : User
    {
        /// <summary>
        /// The player's current status.
        /// </summary>
        /// <seealso cref="IRefereeHubClient.UserStatusChanged"/>
        public MatchUserStatus Status { get; internal set; }

        /// <summary>
        /// The player's current selected mods.
        /// Only applicable if the current <see cref="PlaylistItem"/> specifies any <see cref="PlaylistItem.allowedMods"/>.
        /// </summary>
        /// <seealso cref="IRefereeHubClient.UserModsChanged"/>
        public List<Mod> UserMods { get; internal set; } = [];

        /// <summary>
        /// The player's current team.
        /// Only applicable if the room is in <see cref="MatchType.TeamVersus"/>.
        /// </summary>
        /// <seealso cref="IRefereeHubClient.UserTeamChanged"/>
        public MatchTeam? Team { get; internal set; }

        /// <summary>
        /// The ruleset of the user, if in freestyle.
        /// </summary>
        public Ruleset? Ruleset { get; internal set; }

        /// <summary>
        /// The ID of the beatmap difficulty selected by the user, if in freestyle
        /// </summary>
        public int? BeatmapId { get; internal set; }
    }
}
