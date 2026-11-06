using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KittyRef.Core.Common
{
    public static class EventExtension
    {
        public static void SafeInvoke(this Action? handler, string? eventName = null, ILogger? logger = null)
        {
            if (handler == null) return;

            logger ??= NullLogger.Instance;

            foreach (var subscriber in Delegate.EnumerateInvocationList(handler))
            {
                try
                {
                    subscriber();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Handler for event {EventName} failed", eventName ?? "Unknown");
                }
            }
        }

        public static void SafeInvoke<T>(this Action<T>? handler, T arg, string? eventName = null, ILogger? logger = null)
        {
            if (handler == null) return;

            logger ??= NullLogger.Instance;

            foreach (var subscriber in Delegate.EnumerateInvocationList(handler))
            {
                try
                {
                    subscriber(arg);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Handler for event {EventName} failed", eventName ?? "Unknown");
                }
            }
        }
    }
}
