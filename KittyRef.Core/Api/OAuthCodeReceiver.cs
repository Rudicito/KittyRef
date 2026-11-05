using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KittyRef.Core.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KittyRef.Core.Api
{
    public class OAuthCodeReceiver
    {
        private readonly string redirectUri;
        private const string callback_url = "http://localhost:8098/";
        private const string response_string = "<html><body><h3>Success !</h3></body></html>";

        private readonly HttpListener listener;

        private readonly ILogger<OAuthCodeReceiver> logger;

        public OAuthCodeReceiver(int clientId, string scopes, ILogger<OAuthCodeReceiver>? logger)
        {
            this.logger = logger ?? NullLogger<OAuthCodeReceiver>.Instance;
            redirectUri =
                $"https://osu.ppy.sh/oauth/authorize?client_id={clientId}&response_type=code&scope={WebUtility.UrlEncode(scopes)}";

            listener = new HttpListener();
            listener.Prefixes.Add(callback_url);
        }

        public async Task<string> GetCode()
        {
            listener.Start();
            logger.LogInformation("Listening for login...");

            // Open the website
            OpenExternal.OpenUrlExternally(redirectUri);

            try
            {
                // we wait for osu website response
                var context = await Task.Run(() => listener.GetContext());
                var request = context.Request;
                string code = request.QueryString.Get("code") ?? throw new Exception("Error, web send invalid or missing code");

                // we show a success message
                var response = context.Response;
                byte[] buffer = Encoding.UTF8.GetBytes(response_string);

                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer.AsMemory(0, buffer.Length));
                response.OutputStream.Close();

                logger.LogInformation("Code retrieved successfully !");
                return code;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when listening : {ex.Message}", ex);
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
