using spotify.companion.Helpers;
using System.Threading.Tasks;

namespace spotify.companion.Broker
{
    internal class AuthBroker
    {
        internal static async Task AuthenticateAsync()
        {
            await Api.Spotify.Auth.OAuth.AuthenticateAsync();
        }

        internal static async Task<LoginMessengerHelper> LoginAsync()
        {
            return await Api.Spotify.Auth.OAuth.LoginAsync();
        }

        internal static void Logout()
        {
            Api.Spotify.Auth.OAuth.Logout();
        }
    }
}
