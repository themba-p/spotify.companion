# Origin for Spotify
A simple app to play, merge or delete multiple playlists from your Spotify Library. 
Just testing out the new WinUI 3.

![Screenshot](https://user-images.githubusercontent.com/57948152/150827524-02e5c803-9296-41df-9abd-31449de527a4.jpg)

## Supported features

- Merge multiple playlists.
- Clone a playlist (even if you don't own it).
- Unfollow/Delete multiple playlists (only the ones you created obviously).
- Save your Liked Songs into a playlist.
- Remove all Liked Songs.
- Find and remove duplicate songs from playlists (Only the ones you own again).

## Get up and running...
1. Install [Windows App SDK](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=vs-2022)
2. You gonna need your own clientID and ClientSecret [Credentials.cs](../master/spotify.companion/Api/Spotify/Common/Credentials.cs).\
Get your own credentials at [Spotify Developer dashboard](https://developer.spotify.com/dashboard/) (requires a Spotify account). 
```js
internal static class Credentials
{
    internal static readonly string ClientSecret = "";
    internal static readonly string ClientId = "";
}
```
3. Run and hope it doesn't crash :|

### Created with the help of:

- [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET).
- [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit).
