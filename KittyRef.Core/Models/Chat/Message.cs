using System;
using System.Collections.Generic;

namespace KittyRef.Core.Models.Chat
{
    public class Message : IEquatable<Message>
    {
        public long? Id { get; internal set; }
        public string Uuid { get; internal set; } = string.Empty;
        public int UserId { get; internal set; }
        public string? Username { get; internal set; }
        public string Content { get; internal set; } = string.Empty;
        public DateTimeOffset Timestamp { get; internal set; }

        /// <summary>
        /// The text that is displayed in chat.
        /// </summary>
        public string DisplayContent { get; internal set; } = string.Empty;

        /// <summary>
        /// The links found in this message.
        /// </summary>
        /// <remarks>The <see cref="Link"/>s' <see cref="Index"/> and <see cref="Link.Length"/>s are according to <see cref="DisplayContent"/></remarks>
        public List<Link> Links = [];

        public virtual bool Equals(Message? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id.HasValue && Id == other.Id;
        }
    }
}
