using System.Linq;
using KittyRef.Core.Models;
using KittyRef.Core.Referee.Models.Events;

namespace KittyRef.Core.Referee.Models.Mapping
{
    public static class PlayerMapping
    {
        public static Player FromDto(this PlayerDto dto, Ruleset? ruleset = null)
        {
            ruleset ??= Ruleset.Osu;
            ruleset = dto.Style.RulesetId != null ? RulesetMapping.ToRuleset(dto.Style.RulesetId.Value) : ruleset;

            return new Player
            {
                UserId = dto.UserId,
                Status = dto.Status,
                Ruleset = ruleset,
                BeatmapId = dto.Style.BeatmapId,
                UserMods = dto.UserMods.Select(m => m.FromDto(ruleset.Value)).ToList(),
                Team = dto.Team
            };
        }
    }
}
