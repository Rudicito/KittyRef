using KittyRef.Core.Referee.Models.Events;

namespace KittyRef.Core.Referee.Models.Mapping
{
    public static class RefereeMapping
    {
        public static Core.Models.Referee FromDto(RefereeDto dto)
        {
            return new Core.Models.Referee
            {
                UserId = dto.UserId,
            };
        }
    }
}
