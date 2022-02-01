using spotify.companion.Model;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace spotify.companion.Broker
{
    internal class DataBroker
    {
        private static SpotifyClient SpotifyClient;

        public static void InitializeClient(SpotifyClient spotifyClient)
        {
            SpotifyClient = spotifyClient;
        }

        public static async Task<Playlist> GetPlaylistAsync(string id)
        {
            var playlist = await Api.Spotify.Data.Playlist.GetAsync(SpotifyClient, id);
            return Playlist.Convert(playlist);
        }

        public static async Task<List<Playlist>> GetPlaylistsAsync()
        {
            List<Playlist> items = new();
            var playlists = await Api.Spotify.Data.Playlist.GetAsync(SpotifyClient);
            if (playlists == null) return null;

            foreach (var item in playlists)
            {
                var playlist = Playlist.Convert(item);
                if (playlist != null) items.Add(playlist);
            }
            return items;
        }

        public static async Task<List<string>> GetTracksIdsAsync(string playlistId)
        {
            return await Api.Spotify.Data.Playlist.GetTracksIdsAsync(SpotifyClient, playlistId);
        }

        public static async Task<List<string>> GetTracksUrisAsync(string playlistId)
        {
            return await Api.Spotify.Data.Playlist.GetTracksUrisAsync(SpotifyClient, playlistId);
        }

        public static async Task<bool> AddToPlaylist(string id, IEnumerable<string> uris)
        {
            if (uris.Count() <= 100)
                return await Api.Spotify.Data.Playlist.AddAsync(SpotifyClient, uris, id);
            else
            {
                int index = 0;
                int count = 100;
                int max = uris.Count();
                int sum = max;

                List<string> temp;
                bool success = false;
                while(index < max)
                {
                    temp = uris.ToList().GetRange(index, count);
                    success = await Api.Spotify.Data.Playlist.AddAsync(SpotifyClient, temp, id);
                    index += count;
                    sum -= count;
                    if (sum <= 100) count = sum;
                }
                return success;
            }
        }

        public static async Task DeleteTracksAsync(string id, IEnumerable<string> uris)
        {
            if (uris.Count() < 100)
            {
                await Api.Spotify.Data.Playlist.DeleteTracksAsync(SpotifyClient, id, uris);
            }
            else
            {
                int index = 0;
                int count = 100;
                int max = uris.Count();
                int sum = max;

                List<string> temp;
                while (index < max)
                {
                    temp = uris.ToList().GetRange(index, count);
                    await Api.Spotify.Data.Playlist.DeleteTracksAsync(SpotifyClient, id, temp);
                    index += count;
                    sum -= count;
                    if (sum <= 100) count = sum;
                }
            }
        }

        public static async Task<bool> PlayItems(List<string> uris,int index = 0)
        {
            return await Api.Spotify.Data.Playlist.Play(SpotifyClient, uris, index);
        }

        public static async Task CreatePlaylistAsync(string userId, string name, string description, Action<Task<FullPlaylist>> callback)
        {
            await Api.Spotify.Data.Playlist.CreateAsync(SpotifyClient, userId, name, description, callback);
        }

        public static async Task MergePlaylists(string name, string userId, IEnumerable<string> trackUris, Action<Playlist> callbackFunc)
        {
            if (trackUris == null) callbackFunc(null);

            Action<Task<FullPlaylist>> callback = new(async (result) =>
            {
                if (result != null && result.IsCompletedSuccessfully)
                {
                    FullPlaylist newPlaylist = result.Result;

                    if (newPlaylist == null) callbackFunc(null);

                    var success = await AddToPlaylist(newPlaylist.Id, trackUris);

                    if (success)
                        callbackFunc(await GetPlaylistAsync(newPlaylist.Id));
                    else
                        callbackFunc(null);
                }
            });

            await CreatePlaylistAsync(userId, name, null, callback);

        }

        public static async Task ClonePlaylist(Playlist playlist, string userId, Action<Playlist> callbackFunc)
        {
            if (playlist == null) callbackFunc(null);

            string name = playlist.DisplayName + "(Copy)";

            Action<Task<FullPlaylist>> callback = new(async (result) =>
            {
                if (result != null && result.Result != null)
                {
                    FullPlaylist newPlaylist = result.Result;

                    if (newPlaylist == null) callbackFunc(null);

                    List<string> trackUris = await Api.Spotify.Data.Playlist.GetTracksUrisAsync(SpotifyClient, playlist.Id);

                    if (trackUris == null) callbackFunc(null);

                    var success = await Api.Spotify.Data.Playlist.AddAsync(SpotifyClient, trackUris, newPlaylist.Id);

                    if (success)
                    {
                        callbackFunc(Playlist.Convert(await Api.Spotify.Data.Playlist.GetAsync(SpotifyClient, newPlaylist.Id)));
                    }
                    else
                        callbackFunc(null);
                }
            });

            await CreatePlaylistAsync(userId, name, playlist.Description, callback);
        }

        public static async Task<bool> UnfollowPlaylistAsync(string id)
        {
            return await Api.Spotify.Data.Playlist.UnfollowAsync(SpotifyClient, id);
        }

        public static async Task<List<FullTrack>> GetLikedAsync()
        {
            return await Api.Spotify.Data.Playlist.GetLikedAsync(SpotifyClient);
        }

        public static async Task<bool> ClearLikedAsync(IEnumerable<string> trackIds)
        {
            List<string> ids;
            if (trackIds != null && trackIds.Any()) 
                ids = trackIds.ToList();
            else
                ids = await Api.Spotify.Data.Playlist.GetLikedIdsAsync(SpotifyClient);

            if (ids == null || ids.Count == 0) return false;

            //'Max 50 IDs at once'
            bool result = false;

            if (ids.Count < 50)
            {
                await Api.Spotify.Data.Playlist.RemoveLikedAsync(SpotifyClient, ids);
            }
            else
            {
                int index = 0;
                int count = 50;
                int max = ids.Count;
                int sum = max;

                List<string> temp;
                while (index < max)
                {
                    temp = ids.GetRange(index, count);
                    result = await Api.Spotify.Data.Playlist.RemoveLikedAsync(SpotifyClient, temp); ;
                    index += count;
                    sum -= count;
                    if (sum <= 50) count = sum;
                }
            }

            return result;
        }

        public static async Task<List<TrackCompareItem>> GetTracksToCompareAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            List<FullTrack> collection = await Api.Spotify.Data.Playlist.GetTracksToCompareAsync(SpotifyClient, id);
            if (collection == null) return null;

            List<TrackCompareItem> items = new();
            foreach (var item in collection)
            {
                items.Add(TrackCompareItem.Convert(item));
            }

            return items;
        }

    }
}
