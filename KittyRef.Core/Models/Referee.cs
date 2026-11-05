// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace KittyRef.Core.Models
{
    /// <summary>
    /// Represents a referee in the room.
    /// </summary>
    [PublicAPI]
    public class Referee
    {
        /// <summary>
        /// The ID of the referee.
        /// </summary>
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
    }
}
