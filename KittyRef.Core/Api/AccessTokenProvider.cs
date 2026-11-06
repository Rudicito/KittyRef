using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KittyRef.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using osu.NET.Authorization;

namespace KittyRef.Core.Api;

public class AccessTokenProvider : ITokenSource, IOsuAccessTokenProvider, IDisposable
{
    private const string token_url = "https://osu.ppy.sh/oauth/token";
    private readonly HttpClient httpClient = new();
    private static readonly TimeSpan refresh_margin = TimeSpan.FromMinutes(5);

    private readonly int clientId;
    private readonly string clientSecret;
    private readonly string code;
    private readonly ILogger<AccessTokenProvider> logger;

    private volatile string accessToken = string.Empty;
    private string refreshToken = string.Empty;

    private DateTimeOffset expirationDate = DateTimeOffset.MinValue;

    public event Action<string>? TokenRefreshed;

    public AccessTokenProvider(int clientId, string clientSecret, string code, ILogger<AccessTokenProvider>? logger)
    {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.code = code;
        this.logger = logger ?? new NullLogger<AccessTokenProvider>();
    }

    public string GetAccessToken() => accessToken;
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) => Task.FromResult(accessToken);

    public async Task StartAsync(CancellationToken ct = default)
    {
        await requestAccessToken(ct);
        _ = refreshLoop(ct);
    }

    private async Task refreshLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var delay = expirationDate - DateTimeOffset.UtcNow - refresh_margin;
                if (delay < TimeSpan.FromSeconds(10)) delay = TimeSpan.FromSeconds(10);

                await Task.Delay(delay, ct);

                await fetchRefreshAccessToken(ct);

                logger.LogInformation("Token refreshed, expires at {Expiration}", expirationDate);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Token refresh failed, retrying in 30s");
                try { await Task.Delay(TimeSpan.FromSeconds(30), ct); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private async Task requestAccessToken(CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(token_url, new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["client_id"] = clientId.ToString(),
                    ["client_secret"] = clientSecret,
                    ["code"] = code,
                }), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AccessTokenRetrievalException("Failed to send the authorization request.", ex);
        }

        var osuCodeResponse = await deserializeRequest(response, ct);

        accessToken = osuCodeResponse.AccessToken!;
        refreshToken = osuCodeResponse.RefreshToken!;
        expirationDate = DateTimeOffset.UtcNow.AddSeconds(osuCodeResponse.ExpiresIn!.Value);
    }

    private async Task fetchRefreshAccessToken(CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(token_url, new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = clientId.ToString(),
                    ["client_secret"] = clientSecret,
                    ["refresh_token"] = refreshToken,
                }), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AccessTokenRetrievalException("Failed to send the authorization request.", ex);
        }

        var osuCodeResponse = await deserializeRequest(response, ct);

        accessToken = osuCodeResponse.AccessToken!;
        refreshToken = osuCodeResponse.RefreshToken!;
        expirationDate = DateTimeOffset.UtcNow.AddSeconds(osuCodeResponse.ExpiresIn!.Value);

        TokenRefreshed.SafeInvoke(accessToken, nameof(TokenRefreshed), logger);
    }

    private async Task<OsuCodeTokenResponse> deserializeRequest(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string json = await response.Content.ReadAsStringAsync(cancellationToken);

        OsuCodeTokenResponse accessTokenResponse;
        try
        {
            accessTokenResponse = JsonConvert.DeserializeObject<OsuCodeTokenResponse>(json) ?? throw new("The response is null.");
        }
        catch (Exception ex)
        {
            throw new AccessTokenRetrievalException("Failed to parse the access token response.", ex);
        }

        // If the response is not successful, there should be an error description indicating what went wrong.
        // Also handle the case where the access token or expiration date is null but the response was successful, which should not happen.
        if (!response.IsSuccessStatusCode)
            throw new AccessTokenRetrievalException($"{response.StatusCode}: {accessTokenResponse.ErrorDescription}");
        else if (accessTokenResponse.AccessToken is null || accessTokenResponse.RefreshToken is null || accessTokenResponse.ExpiresIn is null)
            throw new AccessTokenRetrievalException(
                "The access token or expiration date is null, but no error was reported.");

        return accessTokenResponse;
    }

    public void Dispose()
    {
        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
