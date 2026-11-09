namespace KittyRef.Core.Models
{
    public class Referee
    {
        /// <summary>
        /// The ID of the referee.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The name of the referee
        /// </summary>
        public string Username { get; set; } = string.Empty;
    }
}
