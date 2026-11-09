// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace KittyRef.Core.Referee.Models.Events
{
    /// <summary>
    /// A playlist item in a multiplayer room has changed.
    /// </summary>
    [PublicAPI]
    public class PlaylistItemChangedEvent
    {
        /// <summary>
        /// The ID of the room.
        /// </summary>
        [JsonPropertyName("room_id")]
        public long RoomId { get; set; }

        /// <summary>
        /// The playlist item that was changed.
        /// </summary>
        [JsonPropertyName("playlist_item")]
        public PlaylistItem PlaylistItem { get; set; } = null!;
    }
}
