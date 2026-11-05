using Newtonsoft.Json;

namespace KittyRef.Core.Api
{
    public class OsuCodeTokenResponse
    {
        /// <summary>
        /// The access token. If null, an error likely occured.
        /// </summary>
        [JsonProperty("access_token")]
        public string? AccessToken { get; init; }

        /// <summary>
        /// The amount of seconds in which the access token will expire. If null, an error likely occured.
        /// </summary>
        [JsonProperty("expires_in")]
        public int? ExpiresIn { get; init; }

        /// <summary>
        /// The refresh token use to get a new access token. If null, an error likely occured.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string? RefreshToken { get; init; }

        /// <summary>
        /// The description of the authorization error. If null, no error occurred.
        /// </summary>
        [JsonProperty("error_description")]
        public string? ErrorDescription { get; init; }
    }
}
