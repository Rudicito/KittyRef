// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;
using JetBrains.Annotations;
using KittyRef.Core.Referee.Models.Requests;

namespace KittyRef.Core.Referee.Models.Events
{
    /// <summary>
    /// Contains the current state of the room.
    /// </summary>
    [PublicAPI]
    public class MatchState
    {
        /// <summary>
        /// The current match type.
        /// </summary>
        [JsonPropertyName("type")]
        public MatchType Type { get; }

        /// <summary>
        /// Whether the room is currently locked.
        /// When the room is locked, players cannot change teams themselves, only referees can change it for them.
        /// </summary>
        [JsonPropertyName("locked")]
        public bool Locked { get; }

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
        /// <seealso cref="RoomSettingsChangedEvent.MaxParticipants"/>
        [JsonPropertyName("slots")]
        public int?[]? Slots { get; }
    }
}
