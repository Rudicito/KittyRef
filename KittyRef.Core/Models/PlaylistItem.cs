namespace KittyRef.Core.Models
{
    public class PlaylistItem
    {
        /// <summary>
        /// The ID of the playlist item.
        /// </summary>
        public long PlaylistItemId { get; set; }

        /// <summary>
        /// The ID of the ruleset of the playlist item.
        /// </summary>
        public int RulesetId { get; set; }

        /// <summary>
        /// The ID of the beatmap of the playlist item.
        /// </summary>
        public int BeatmapId { get; set; }

        /// <summary>
        /// The mods required by this playlist item.
        /// </summary>
        public Mod[] RequiredMods { get; set; } = [];

        /// <summary>
        /// The mods allowed for player selection by this playlist item.
        /// </summary>
        public Mod[] AllowedMods { get; set; } = [];

        /// <summary>
        /// Whether this playlist item permits players to freestyle.
        /// </summary>
        public bool Freestyle { get; set; }

        /// <summary>
        /// Whether this playlist item has already been played.
        /// </summary>
        public bool WasPlayed { get; set; }

        /// <summary>
        /// The order of this item in the room's playlist.
        /// Lower is earlier.
        /// </summary>
        public int Order { get; set; }
    }
}
