using System;

namespace KittyRef.Core.Api;

public interface ITokenSource
{
    string GetAccessToken();

    event Action<string>? TokenRefreshed;
}
