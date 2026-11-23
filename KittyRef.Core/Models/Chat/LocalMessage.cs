using System;

namespace KittyRef.Core.Models.Chat
{
    public class LocalMessage : Message
    {
        public LocalMessage(string message)
        {
            Timestamp = DateTimeOffset.Now;
            Content = message;
        }
    }
}
