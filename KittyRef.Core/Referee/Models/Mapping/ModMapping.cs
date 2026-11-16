using System;
using System.Linq;
using System.Text.Json;
using KittyRef.Core.Models;
using KittyRef.Core.Models.Mods;

namespace KittyRef.Core.Referee.Models.Mapping
{
    public static class ModMapping
    {
        public static Mod FromDto(this ModDto modDto, Ruleset ruleset)
        {
            var kittyMod = ModRegistry.FindByAcronym(modDto.Acronym, ruleset);
            if (kittyMod == null) throw new Exception($"Unknown Mod received from the server : {modDto.Acronym}");

            if (modDto.Settings == null) return kittyMod;

            foreach ((string name, object raw) in modDto.Settings)
            {
                var setting = kittyMod.Settings.FirstOrDefault(s => s.Name == name)
                              ?? throw new InvalidOperationException($"Unknown setting '{name}' on mod {modDto.Acronym}");

                if (raw is not JsonElement json || !setting.trySetFromJson(json))
                    throw new InvalidOperationException($"Invalid value for '{name}' on mod {modDto.Acronym}: {raw}");
            }

            return kittyMod;
        }

        public static ModDto ToDto(Mod mod)
        {
            var changed = mod.Settings.Where(s => !s.IsDefault).ToList();

            return new ModDto
            {
                Acronym = mod.Acronym,
                Settings = changed.Count > 0
                    ? changed.ToDictionary(ms => ms.Name, ms => ms.ObjectValue!)
                    : null
            };
        }
    }
}
