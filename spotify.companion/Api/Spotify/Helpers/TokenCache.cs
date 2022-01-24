using spotify.companion.Api.Spotify.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace spotify.companion.Api.Spotify.Helpers
{
    internal class TokenCache
    {
        internal static async Task SaveAsync(string JSON)
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(Constants.tokenStoreName, CreationCollisionOption.ReplaceExisting);
            await File.WriteAllTextAsync(Constants.tokenStorePath, JSON);
        }

        internal static async Task<string> GetAsync()
        {
            if (!File.Exists(Constants.tokenStorePath)) return null;

            return await File.ReadAllTextAsync(Constants.tokenStorePath);
        }

        internal static void Delete()
        {
            File.Delete(Constants.tokenStorePath);
        }
    }
}
