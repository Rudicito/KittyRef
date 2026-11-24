using System;

namespace KittyRef.Core.Models
{
    public class Countdown
    {
        /// <summary>
        /// The ID of the countdown.
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Number of seconds (fractional) until the completion of the countdown.
        /// </summary>
        public double Seconds { get; init; }

        /// <summary>
        /// The point in time when the countdown started.
        /// </summary>
        public DateTimeOffset StartedAt { get; init; }

        /// <summary>
        /// The point in time when the countdown ends.
        /// </summary>
        public DateTimeOffset EndsAt { get; init; }

        public Countdown(long id, double seconds, DateTimeOffset? startedAt = null)
        {
            startedAt ??= DateTimeOffset.UtcNow;

            Id = id;
            Seconds = seconds;
            StartedAt = startedAt.Value;
            EndsAt = startedAt.Value.AddSeconds(Math.Max(0, seconds));
        }
    }
}
