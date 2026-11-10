using System;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Difficulty;

namespace KittyRef.Generators
{
    public static class LegacyHelper
    {
        public static Ruleset GetRulesetFromLegacyID(int id)
        {
            switch (id)
            {
                default:
                    throw new ArgumentException("Invalid ruleset ID provided.");

                case 0:
                    return new OsuRuleset();

                case 1:
                    return new TaikoRuleset();

                case 2:
                    return new CatchRuleset();

                case 3:
                    return new ManiaRuleset();
            }
        }

        public static string GetRulesetShortNameFromId(int id)
        {
            switch (id)
            {
                default:
                    throw new ArgumentException("Invalid ruleset ID provided.");

                case 0:
                    return "osu";

                case 1:
                    return "taiko";

                case 2:
                    return "fruits";

                case 3:
                    return "mania";
            }
        }

        public static DifficultyAttributes CreateDifficultyAttributes(int legacyId)
        {
            switch (legacyId)
            {
                case 0:
                    return new OsuDifficultyAttributes();

                case 1:
                    return new TaikoDifficultyAttributes();

                case 2:
                    return new CatchDifficultyAttributes();

                case 3:
                    return new ManiaDifficultyAttributes();

                default:
                    throw new ArgumentException($"Invalid ruleset ID: {legacyId}", nameof(legacyId));
            }
        }
    }
}
