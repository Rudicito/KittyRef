// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using KittyRef.Core.Utils;

namespace KittyRef.Core.Models.Chat;

/// <remarks>
/// Based on https://github.com/ppy/osu/blob/c834803ea9988159d67e1dc5d99cfaccada685d9/osu.Game/Online/Chat/Channel.cs
/// </remarks>
public class ChatChannel
{
    public const int MAX_HISTORY = 300;

    public IReadOnlyList<Message> Messages => messages;

    private readonly SortedList<Message> messages = new SortedList<Message>(Comparer<Message>.Default);

    private readonly List<LocalMessage> pendingMessages = [];

    internal ChatChannel(long id)
    {
        Id = id;
    }

    public long Id { get; }

    /// <summary>
    /// An event that fires when new messages arrived.
    /// </summary>
    public event Action<IEnumerable<Message>>? NewMessagesArrived;

    /// <summary>
    /// An event that fires when a pending message gets resolved.
    /// </summary>
    public event Action<LocalMessage, Message>? PendingMessageResolved;

    /// <summary>
    /// An event that fires when a pending message gets removed.
    /// </summary>
    public event Action<Message>? MessageRemoved;

    /// <summary>
    /// Adds the argument message as a local echo. When this local echo is resolved <see cref="PendingMessageResolved"/> will get called.
    /// </summary>
    /// <param name="message"></param>
    internal void addLocal(LocalMessage message)
    {
        pendingMessages.Add(message);
        messages.Add(message);

        NewMessagesArrived?.Invoke([message]);
    }

    /// <summary>
    /// Adds new messages to the channel and purges old messages. Triggers the <see cref="NewMessagesArrived"/> event.
    /// </summary>
    /// <param name="incoming"></param>
    internal void addNewMessages(params Message[] incoming)
    {
        foreach (var m in incoming)
        {
            var localMessage = pendingMessages.FirstOrDefault(local => local.Uuid == m.Uuid);

            if (localMessage != null)
                replaceMessage(localMessage, m);
        }

        incoming = messages.Except(incoming).ToArray();

        if (incoming.Length == 0) return;

        messages.AddRange(incoming);

        long? maxMessageId = incoming.Max(m => m.Id);
        if (lastMessageId == null || maxMessageId > lastMessageId)
            lastMessageId = maxMessageId;

        purgeOldMessages();

        NewMessagesArrived?.Invoke(incoming);
    }

    internal void removeMessagesFromUser(int userId)
    {
        for (int i = 0; i < Messages.Count; i++)
        {
            var message = Messages[i];

            if (message.UserId == userId)
            {
                messages.RemoveAt(i--);
                MessageRemoved?.Invoke(message);
            }
        }
    }

    private long? lastMessageId { get; set; }

    /// <summary>
    /// Replace or remove a message from the channel.
    /// </summary>
    /// <param name="echo">The local echo message (client-side).</param>
    /// <param name="final">The response message, or null if the message became invalid.</param>
    private void replaceMessage(LocalMessage echo, Message? final)
    {
        if (!pendingMessages.Remove(echo))
            throw new InvalidOperationException("Attempted to remove echo that wasn't present");

        messages.Remove(echo);

        if (final == null)
        {
            MessageRemoved?.Invoke(echo);
            return;
        }

        if (Messages.Contains(final))
            throw new InvalidOperationException("Attempted to add the same message again");

        messages.Add(final);

        if (final.Id > lastMessageId)
            lastMessageId = final.Id;

        PendingMessageResolved?.Invoke(echo, final);
    }

    private void purgeOldMessages()
    {
        // never purge local echos
        int messageCount = Messages.Count - pendingMessages.Count;
        if (messageCount > MAX_HISTORY)
            messages.RemoveRange(0, messageCount - MAX_HISTORY);
    }
}
