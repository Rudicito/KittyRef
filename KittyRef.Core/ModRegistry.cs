using System.Collections.Generic;
using System.Linq;
using KittyRef.Core.Models;
using KittyRef.Core.Models.Mods;

namespace KittyRef.Core
{
    public static class ModRegistry
    {
        private static IReadOnlyDictionary<Ruleset, Mod[]> allMods { get; } = ModCatalog.CreateAll()
            .GroupBy(m => m.Ruleset)
            .ToDictionary(g => g.Key, g => g.ToArray());

        public static Mod? FindByAcronym(Ruleset ruleset, string acronym)
            => allMods[ruleset].FirstOrDefault(m => m.Acronym == acronym);

        public static IEnumerable<Mod> GetForRuleset(Ruleset ruleset)
            => allMods[ruleset];
    }
}
