using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace KittyRef.Core.Referee
{
    public partial class RefereeHubException : Exception
    {
        public int Code { get; }
        public string ErrorMessage { get; }

        public RefereeHubException(int code, string message) : base($"Error {code}: {message}")
        {
            Code = code;
            ErrorMessage = message;
        }

        public static RefereeHubException? TryParse(HubException ex, ILogger? logger = null)
        {
            try
            {
                var m = errorRegex().Match(ex.Message);

                if (!m.Success || !int.TryParse(m.Groups[1].Value, out int code))
                    return null;

                return new RefereeHubException(code, m.Groups[2].Value);
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Exception occured while parsing a referee hub exception: {Message}", ex.Message);
                return null;
            }
        }

        [GeneratedRegex(@"^Error (\d+): (.*)$", RegexOptions.Singleline)]
        private static partial Regex errorRegex();
    }
}
