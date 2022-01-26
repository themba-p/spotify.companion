using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SpotifyAPI.Web.Scopes;
using Microsoft.Toolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using spotify.companion.Model;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using spotify.companion.Api.Spotify.Common;
using spotify.companion.Api.Spotify.Helpers;
using spotify.companion.Enums;
using spotify.companion.Helpers;

namespace spotify.companion.Api.Spotify.Auth
{
    internal class OAuth
    {
        private static EmbedIOAuthServer _server;

        private static void SendMessage(ResponseType responseType, SpotifyClient spotifyClient, PrivateUser user = null)
        {
            LoginMessengerHelper loginResponse = new(responseType, spotifyClient, User.Convert(user));
            WeakReferenceMessenger.Default.Send(loginResponse);
        }

        public static async Task<LoginMessengerHelper> LoginAsync()
        {
            try
            {
                string json = await TokenCache.GetAsync();

                if (json == null)
                {
                    SendMessage(ResponseType.Error, null);
                    return new LoginMessengerHelper(ResponseType.Error, null, null);
                }

                AuthorizationCodeTokenResponse token = JsonConvert.DeserializeObject<AuthorizationCodeTokenResponse>(json);

                var authenticator = new AuthorizationCodeAuthenticator(Credentials.ClientId, Credentials.ClientSecret, token);
                authenticator.TokenRefreshed += async (sender, tokenx) => await TokenCache.SaveAsync(JsonConvert.SerializeObject(tokenx));

                var config = SpotifyClientConfig.CreateDefault()
                  .WithAuthenticator(authenticator)
                  .WithRetryHandler(new SimpleRetryHandler() { RetryAfter = TimeSpan.FromSeconds(1) });

                SpotifyClient client = new(config);
                PrivateUser user = await client.UserProfile.Current();

                ResponseType responseType = (user != null) ? ResponseType.Success : ResponseType.Error;

                if (responseType == ResponseType.Error) TokenCache.Delete();

                return new LoginMessengerHelper(responseType, client, User.Convert(user));
            }
            catch (Exception)
            {
                return new LoginMessengerHelper(ResponseType.Error, null, null);
            }

        }

        public static void Logout()
        {
            TokenCache.Delete();
        }

        public static async Task AuthenticateAsync()
        {
            _server = new EmbedIOAuthServer(new Uri(Constants.callbackUrl), Constants.callbackPort);
            await _server.Start();

            _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
            _server.ErrorReceived += OnErrorReceived;
            
            var request = new LoginRequest(_server.BaseUri, Credentials.ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { UserReadPrivate, PlaylistReadPrivate,
                    UserLibraryModify, UserLibraryRead, PlaylistModifyPrivate,
                    PlaylistModifyPublic, UserReadEmail }
            };
            BrowserUtil.Open(request.ToUri());
        }

        private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            await _server.Stop();
            _server.Dispose();

            var config = SpotifyClientConfig
                .CreateDefault()
                .WithRetryHandler(new SimpleRetryHandler() { RetryAfter = TimeSpan.FromSeconds(1) });

            var tokenResponse = await new OAuthClient(config).RequestToken(
              new AuthorizationCodeTokenRequest(
                Credentials.ClientId, Credentials.ClientSecret, response.Code, new Uri(Constants.callbackUrl)
              )
            );

            await TokenCache.SaveAsync(JsonConvert.SerializeObject(tokenResponse));

            SpotifyClient client = new(tokenResponse.AccessToken);
            PrivateUser user = await client.UserProfile.Current();

            SendMessage(ResponseType.Success, client, user);
            
        }

        private static async Task OnErrorReceived(object sender, string error, string state)
        {
            Console.WriteLine($"Aborting authorization, error received: {error}");
            TokenCache.Delete();
            await _server.Stop();

            ResponseType responseType = ResponseType.Error;
            if (error == "access_denied") responseType = ResponseType.Cancelled;
            SendMessage(responseType, null);
        }
    }

    
}
