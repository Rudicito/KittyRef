namespace KittyRef.Core.Models
{
    public class Referee
    {
        /// <summary>
        /// The ID of the referee.
        /// </summary>
        public int UserId { get; internal set; }

        /// <summary>
        /// The name of the referee
        /// </summary>
        public string Username { get; internal set; } = string.Empty;
    }
}
