// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace KittyRef.Core.Referee.Models.Events
{
    /// <summary>
    /// Represents a playlist item.
    /// </summary>
    [PublicAPI]
    public class PlaylistItem
    {
        /// <summary>
        /// The ID of the playlist item.
        /// </summary>
        [JsonPropertyName("id")]
        public long PlaylistItemId { get; set; }

        /// <summary>
        /// The ID of the ruleset of the playlist item.
        /// </summary>
        [JsonPropertyName("ruleset_id")]
        public int RulesetId { get; set; }

        /// <summary>
        /// The ID of the beatmap of the playlist item.
        /// </summary>
        [JsonPropertyName("beatmap_id")]
        public int BeatmapId { get; set; }

        /// <summary>
        /// The mods required by this playlist item.
        /// </summary>
        [JsonPropertyName("required_mods")]
        public Mod[] RequiredMods { get; set; } = [];

        /// <summary>
        /// The mods allowed for player selection by this playlist item.
        /// </summary>
        [JsonPropertyName("allowed_mods")]
        public Mod[] AllowedMods { get; set; } = [];

        /// <summary>
        /// Whether this playlist item permits players to freestyle.
        /// </summary>
        [JsonPropertyName("freestyle")]
        public bool Freestyle { get; set; }

        /// <summary>
        /// Whether this playlist item has already been played.
        /// </summary>
        [JsonPropertyName("was_played")]
        public bool WasPlayed { get; set; }

        /// <summary>
        /// The order of this item in the room's playlist.
        /// Lower is earlier.
        /// </summary>
        [JsonPropertyName("order")]
        public int Order { get; set; }
    }
}
