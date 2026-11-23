using System.Threading;
using System.Threading.Tasks;

namespace KittyRef.Core.Models
{
    /// <summary>
    /// osu! API methods used by a room.
    /// </summary>
    public interface IApiRoom
    {
        Task<bool> SendMessage(long channelId, string message, bool isAction = false, CancellationToken? cancellationToken = null);
    }
}
