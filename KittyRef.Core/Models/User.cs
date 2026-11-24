namespace KittyRef.Core.Models
{
    public class User
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public int UserId { get; internal set; }

        /// <summary>
        /// The name of the user.
        /// </summary>
        public string? Username { get; internal set; }
    }
}
