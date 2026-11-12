using KittyRef.Core.Referee.Models;

namespace KittyRef.Core.Models
{
    public class PlaylistItem
    {
        /// <summary>
        /// The ID of the playlist item.
        /// </summary>
        public long PlaylistItemId { get; internal set; }

        /// <summary>
        /// The ID of the ruleinternal set of the playlist item.
        /// </summary>
        public int RulesetId { get; internal set; }

        /// <summary>
        /// The ID of the beatmap of the playlist item.
        /// </summary>
        public int BeatmapId { get; internal set; }

        /// <summary>
        /// The mods required by this playlist item.
        /// </summary>
        public Mod[] RequiredMods { get; internal set; } = [];

        /// <summary>
        /// The mods allowed for player selection by this playlist item.
        /// </summary>
        public Mod[] AllowedMods { get; internal set; } = [];

        /// <summary>
        /// Whether this playlist item permits players to freestyle.
        /// </summary>
        public bool Freestyle { get; internal set; }

        /// <summary>
        /// Whether this playlist item has already been played.
        /// </summary>
        public bool WasPlayed { get; internal set; }

        /// <summary>
        /// The order of this item in the room's playlist.
        /// Lower is earlier.
        /// </summary>
        public int Order { get; internal set; }
    }
}
