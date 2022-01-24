using Windows.Storage;

namespace spotify.companion.Api.Spotify.Common
{
    internal static class Constants
    {
        public static readonly string tokenStoreName = "spotify_tkn.json";
        private static readonly string localFolderPath = ApplicationData.Current.LocalFolder.Path;
        public static readonly string tokenStorePath = string.Concat(localFolderPath, "\\", tokenStoreName);

        internal static readonly string callbackUrl = "http://localhost:5000/callback";
        internal static readonly int callbackPort = 5000;
    }
}
