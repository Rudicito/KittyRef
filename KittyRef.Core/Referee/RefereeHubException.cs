using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;

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

        public static RefereeHubException? TryParse(HubException ex)
        {
            var m = errorRegex().Match(ex.Message);

            return m.Success
                ? new RefereeHubException(int.Parse(m.Groups[1].Value), m.Groups[2].Value)
                : null;
        }

        [GeneratedRegex(@"^Error (\d+): (.*)$", RegexOptions.Singleline)]
        private static partial Regex errorRegex();
    }
}
