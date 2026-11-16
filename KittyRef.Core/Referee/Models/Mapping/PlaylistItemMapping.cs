using System.Linq;
using KittyRef.Core.Models;
using KittyRef.Core.Referee.Models.Events;

namespace KittyRef.Core.Referee.Models.Mapping
{
    public static class PlaylistItemMapping
    {
        public static PlaylistItem FromDto(this PlaylistItemDto dto)
        {
            var ruleset = RulesetMapping.ToRuleset(dto.RulesetId);
            return new PlaylistItem
            {
                PlaylistItemId = dto.PlaylistItemId,
                Ruleset = ruleset,
                BeatmapId = dto.BeatmapId,
                requiredMods = dto.RequiredMods.Select(m => m.FromDto(ruleset)).ToList(),
                allowedMods = dto.AllowedMods.Select(m => m.FromDto(ruleset)).ToList(),
                Freestyle = dto.Freestyle,
                WasPlayed = dto.WasPlayed,
                Order = dto.Order,
            };
        }

        public static PlaylistItemDto ToDto(this PlaylistItem pi)
        {
            return new PlaylistItemDto
            {
                PlaylistItemId = pi.PlaylistItemId,
                RulesetId = RulesetMapping.FromRuleset(pi.Ruleset),
                BeatmapId = pi.BeatmapId,
                RequiredMods = pi.RequiredMods.Select(ModMapping.ToDto).ToArray(),
                AllowedMods = pi.allowedMods.Select(ModMapping.ToDto).ToArray(),
                Freestyle = pi.Freestyle,
                WasPlayed = pi.WasPlayed,
                Order = pi.Order
            };
        }
    }
}
