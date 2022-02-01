using Microsoft.Toolkit.Mvvm.Messaging;
using spotify.companion.Helpers;
using spotify.companion.Model;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace spotify.companion.Api.Spotify.Data
{
    public class Playlist
    {
        private static void HandleError(SpotifyClient SpotifyClient)
        {
            var statusCode = SpotifyClient.LastResponse.StatusCode;
            if (statusCode == HttpStatusCode.Unauthorized)
                WeakReferenceMessenger.Default.Send(new NavMessengerHelper(Enums.NavTargetType.Login));
            else
            {
                InAppNotification notification = new(Enums.ResponseType.Error, "Unknown error, please try again!", "", false);
                WeakReferenceMessenger.Default.Send(notification);
            }
        }

        public static async Task<List<FullTrack>> GetTracksToCompareAsync(SpotifyClient SpotifyClient, string id)
        {
            try
            {
                PlaylistGetItemsRequest request = new();
                request.Fields.Add("items(track)");

                List<FullTrack> tracks = new();
                var page = await SpotifyClient.Playlists.GetItems(id, request);

                await foreach (var item in SpotifyClient.Paginate(page))
                {
                    if (item.Track is FullTrack fullTrack)
                        tracks.Add(fullTrack);
                }

                return tracks;
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return null;
            }
        }

        public static async Task<List<SimplePlaylist>> GetAsync(SpotifyClient SpotifyClient)
        {
            try
            {
                List<SimplePlaylist> simplePlaylists = new();
                
                var page = await SpotifyClient.Playlists.CurrentUsers();
                await foreach (var item in SpotifyClient.Paginate(page))
                {
                    simplePlaylists.Add(item);
                }

                return simplePlaylists;
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return null;
            }
        }

        public static async Task<FullPlaylist> GetAsync(SpotifyClient SpotifyClient, string id)
        {
            try
            {
                return await SpotifyClient.Playlists.Get(id);
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return null;
            }
        }

        public static async Task<SnapshotResponse> DeleteTracksAsync(SpotifyClient SpotifyClient, string id, IEnumerable<string> uris)
        {
            try
            {
                List<PlaylistRemoveItemsRequest.Item> items = new();
                uris.ToList().ForEach(uri => items.Add(new PlaylistRemoveItemsRequest.Item { Uri = uri }));
                PlaylistRemoveItemsRequest request = new()
                {
                    Tracks = items
                };

                return await SpotifyClient.Playlists.RemoveItems(id, request);
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return null;
            }
        }

        public static async Task<List<string>> GetTracksIdsAsync(SpotifyClient SpotifyClient, string id)
        {
            return await GetTracksPropAsync(SpotifyClient, id, "id");
        }

        public static async Task<List<string>> GetTracksUrisAsync(SpotifyClient SpotifyClient, string id)
        {
            return await GetTracksPropAsync(SpotifyClient, id, "uri");
        }

        private static async Task<List<string>> GetTracksPropAsync(SpotifyClient SpotifyClient, string id, string prop)
        {
            PlaylistGetItemsRequest request = new();
            request.Fields.Add("items(track(" + prop + ", type)), next");

            try
            {
                List<string> uris = new();
                var page = await SpotifyClient.Playlists.GetItems(id, request);

                Action<Paging<PlaylistTrack<IPlayableItem>>> processPage = new(async (page) =>
                {
                    string uri;
                    await foreach (var item in SpotifyClient.Paginate(page))
                    {
                        if (item.Track is FullTrack fullTrack)
                        {
                            uri = fullTrack.Uri;
                            if (!uris.Contains(uri)) uris.Add(uri);
                        }
                    }
                });

                while (page != null)
                {
                    processPage(page);
                    if (page.Next == null) break;
                    page = await SpotifyClient.NextPage(page);
                }

                return uris;
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return null;
            }
        }

        public static async Task CreateAsync(SpotifyClient SpotifyClient, string userId, string name, string description, Action<Task<FullPlaylist>> callback)
        {
            PlaylistCreateRequest request = new(name);
            if (!string.IsNullOrEmpty(description)) request.Description = description;

            try
            {
                await SpotifyClient.Playlists.Create(userId, request).ContinueWith(callback);
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
            }
        }

        internal static async Task<bool> AddAsync(SpotifyClient SpotifyClient, IEnumerable<string> trackUris, string playlistId)
        {
            PlaylistAddItemsRequest request = new(trackUris.ToList());
            try
            {
                return (await SpotifyClient.Playlists.AddItems(playlistId, request) != null);

            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return false;

            }
        }

        /// <summary>
        /// //Unfollows (Delete if user is owner) a list of playlists that current user follows.
        /// </summary>
        /// <param name="ids">
        /// The list of playlist ids the user wants to unfollow.
        /// </param>
        /// <returns>
        /// A list of ids of the playlists that were succesfuly unfollowed.
        /// </returns>
        public static async Task<bool> UnfollowAsync(SpotifyClient SpotifyClient, string id)
        {
            try
            {
                return await SpotifyClient.Follow.UnfollowPlaylist(id);
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return false;
            }
        }

        public static async Task<List<string>> GetLikedIdsAsync(SpotifyClient SpotifyClient)
        {
            try
            {
                List<string> ids = new();
                var page = await SpotifyClient.Library.GetTracks();

                await foreach (var item in SpotifyClient.Paginate(page))
                {
                    if (item.Track is FullTrack fullTrack)
                        ids.Add(fullTrack.Id);
                }

                return ids;
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return null;
            }
        }

        public static async Task<List<FullTrack>> GetLikedAsync(SpotifyClient SpotifyClient)
        {
            try
            {
                List<FullTrack> tracks = new();
                var page = await SpotifyClient.Library.GetTracks();

                await foreach (var item in SpotifyClient.Paginate(page))
                {
                    if (item.Track is FullTrack fullTrack)
                        tracks.Add(fullTrack);
                }

                return tracks;
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return null;
            }
        }

        public static async Task<bool> RemoveLikedAsync(SpotifyClient SpotifyClient, List<string> ids)
        {
            try
            {
                LibraryRemoveTracksRequest request = new(ids);

                return await SpotifyClient.Library.RemoveTracks(request);
            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return false;
            }
        }

        public static async Task<bool> Play(SpotifyClient SpotifyClient, List<string> uris, int index)
        {
            

            try
            {
                PlayerResumePlaybackRequest request = new()
                {
                    Uris = uris,
                    OffsetParam = new PlayerResumePlaybackRequest.Offset { Position = index },
                };

                return await SpotifyClient.Player.ResumePlayback(request);

            }
            catch (Exception)
            {
                HandleError(SpotifyClient);
                return false;
            }
        }
    }
}
