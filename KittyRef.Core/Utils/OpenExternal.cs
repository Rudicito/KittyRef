using System;
using System.Diagnostics;

namespace KittyRef.Core.Utils
{
    public static class OpenExternal
    {
        public static void OpenUrlExternally(string url)
        {
            if (!url.checkIsValidUrl())
                throw new ArgumentException("The provided URL must be one of either http://, https:// or mailto: protocols.", nameof(url));

            openUsingShellExecute(url);
        }

        private static void openUsingShellExecute(string path) => Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true //see https://github.com/dotnet/corefx/issues/10361
        });

        /// <summary>
        /// Checks whether the provided URL is a safe protocol to execute a system <see cref="Process.Start()"/> call with.
        /// </summary>
        /// <remarks>
        /// For now, http://, https:// and mailto: are supported.
        /// More protocols can be added if a use case comes up.
        /// </remarks>
        /// <param name="url">The URL to check.</param>
        /// <returns>Whether the URL is safe to open.</returns>
        private static bool checkIsValidUrl(this string url)
        {
            return url.StartsWith("https://", StringComparison.Ordinal)
                   || url.StartsWith("http://", StringComparison.Ordinal)
                   || url.StartsWith("mailto:", StringComparison.Ordinal);
        }
    }
}
