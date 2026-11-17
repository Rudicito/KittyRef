using KittyRef.Core.Referee.Models;

namespace KittyRef.Core.Models
{
    public class Countdown
    {
        /// <summary>
        /// The ID of the countdown.
        /// </summary>
        public long CountdownId { get; internal set; }

        /// <summary>
        /// Number of seconds (fractional) until the completion of the countdown.
        /// </summary>
        public double Seconds { get; internal set; }

        /// <summary>
        /// The type of the countdown.
        /// </summary>
        public CountdownType Type { get; internal set; }
    }
}
