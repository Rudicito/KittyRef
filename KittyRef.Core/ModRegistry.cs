using System;
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

        private static Mod createNew(this Mod mod) => (Mod)Activator.CreateInstance(mod.GetType(), nonPublic: true)!;

        public static Mod? FindByAcronym(string acronym, Ruleset ruleset)
            => allMods[ruleset].FirstOrDefault(m => m.Acronym == acronym)?.createNew();

        public static IEnumerable<Mod> GetForRuleset(Ruleset ruleset)
            => allMods[ruleset].Select(m => m.createNew());
    }
}
