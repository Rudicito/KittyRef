using System;
using System.Collections.Generic;
using KittyRef.Core.Models.Mods.Settings;

namespace KittyRef.Core.Models.Mods
{
    public abstract class Mod
    {
        public abstract string Acronym  { get; }
        public abstract string Name { get; }

        public abstract Ruleset Ruleset { get; }

        public abstract ModType Type { get; }

        public abstract IReadOnlyList<Type> IncompatibleMods { get; }

        public abstract IReadOnlyList<ModSetting> Settings { get; }

        /// <summary>
        /// Whether this mod is playable by a real human user.
        /// Should be <c>false</c> for cases where the user is not interacting with the game (so it can be excluded from multiplayer selection, for example).
        /// </summary>
        public abstract bool UserPlayable { get; }

        /// <summary>
        /// Whether this mod is valid for multiplayer matches.
        /// Should be <c>false</c> for mods that make gameplay duration dependent on user input (e.g. <c>ModAdaptiveSpeed</c>).
        /// </summary>
        public abstract bool ValidForMultiplayer { get; }

        /// <summary>
        /// Whether this mod is valid as a required mod when freestyle is enabled.
        /// Should be <c>true</c> for mods that are guaranteed to be implemented across all rulesets.
        /// </summary>
        public abstract bool ValidForFreestyleAsRequiredMod { get; }

        /// <summary>
        /// Whether this mod is valid as a free mod in multiplayer matches.
        /// Should be <c>false</c> for mods that affect the gameplay duration (e.g. <c>ModRateAdjust</c> and <c>ModTimeRamp</c>).
        /// </summary>
        public abstract bool ValidForMultiplayerAsFreeMod { get; }

        /// <summary>
        /// Indicates that this mod is always permitted in scenarios wherein a user is submitting a score regardless of other circumstances.
        /// Intended for mods that are informational in nature and do not really affect gameplay by themselves,
        /// but are more of a gauge of increased/decreased difficulty due to the user's configuration (e.g. <c>ModTouchDevice</c>).
        /// </summary>
        public abstract bool AlwaysValidForSubmission { get; }

        /// <summary>
        /// Whether scores with this mod active can give performance points.
        /// </summary>
        public abstract bool Ranked { get; }
    }
}
