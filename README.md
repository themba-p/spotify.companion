# Spotify Companion (aka Origin I suck at naming things)
A simple app to play, merge or delete multiple playlists from your Spotify Library. 
Just testing out the new WinUI 3.

## Supported features

- Merge multiple playlists.
- Clone a playlist (even if you don't own it).
- Unfollow/Delete multiple playlists (only the ones you created obviously).
- Save your Liked Songs into a playlist.
- Remoe all Liked Songs.
- Find and remove duplicate songs from playlists (Only the ones you own again).

## Get up and running...

You gonna need your own clientID and ClientSecret [Credentials.cs](../master/spotify.companion/Api/Spotify/Common/Credentials.cs).\
Get your own credentials at [Spotify Developer dashboard](https://developer.spotify.com/dashboard/) (requires a Spotify account). 
```js
internal static class Credentials
{
    internal static readonly string ClientSecret = "";
    internal static readonly string ClientId = "";
}
```

### Created with the help of:

- [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET).
- [Windows Community Toolkit](https://github.com/CommunityToolkit/WindowsCommunityToolkit).
