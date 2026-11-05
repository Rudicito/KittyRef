using KittyRef.Core.Api;
using osu.NET;

namespace KittyRef.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Test1()
    {
        //todo: use a .env or something
        string scopes = "public identify multiplayer.write_manage chat.read chat.write";
        var oauth = new OAuthCodeReceiver(1, scopes, null);
        string code = await oauth.GetCode();
        var accessTokenProvider = new AccessTokenProvider(1, "secret", code, null);
        await accessTokenProvider.StartAsync();
        var clientApi = new OsuApiClient(accessTokenProvider, null);
        await clientApi.GetChannel(5);

        Assert.Pass();
    }
}
