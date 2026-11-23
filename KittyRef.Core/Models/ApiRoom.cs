using System.Threading;
using System.Threading.Tasks;
using osu.NET;

namespace KittyRef.Core.Models
{
    public class ApiRoom(OsuApiClient api) : IApiRoom
    {
        public async Task<bool> SendMessage(long channelId, string message, bool isAction = false, CancellationToken? cancellationToken = null)
        {
            var result = await api.SendMessageToChannel(channelId, message, isAction, cancellationToken);
            return result.IsSuccess;
        }
    }
}
