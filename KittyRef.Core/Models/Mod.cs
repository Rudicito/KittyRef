// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace KittyRef.Core.Models
{
    /// <summary>
    /// Represents a configured mod.
    /// </summary>
    [PublicAPI]
    public class Mod
    {
        /// <summary>
        /// The acronym of the mod.
        /// </summary>
        [JsonPropertyName("acronym")]
        public string Acronym { get; set; } = string.Empty;

        /// <summary>
        /// The mod settings.
        /// Should not be provided when in the context of specifying allowed / free mods.
        /// </summary>
        [JsonPropertyName("settings")]
        public Dictionary<string, object>? Settings { get; set; }
    }
}
