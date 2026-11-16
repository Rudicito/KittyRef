using System;
using KittyRef.Core.Models;

namespace KittyRef.Core.Referee.Models.Mapping
{
    public static class RulesetMapping
    {
        public static Ruleset ToRuleset(int rulesetId)
        {
            return rulesetId switch
            {
                0 => Ruleset.Osu,
                1 => Ruleset.Taiko,
                2 => Ruleset.Catch,
                3 => Ruleset.Mania,
                _ => throw new ArgumentException($"Invalid ruleset ID provided : {rulesetId}")
            };
        }

        public static int FromRuleset(Ruleset ruleset)
        {
            return ruleset switch
            {
                Ruleset.Osu => 0,
                Ruleset.Taiko => 1,
                Ruleset.Catch => 2,
                Ruleset.Mania => 3,
                _ => throw new ArgumentException($"Invalid ruleset enum provided : {ruleset}")
            };
        }
    }
}
