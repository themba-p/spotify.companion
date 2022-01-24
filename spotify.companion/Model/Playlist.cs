using spotify.companion.Enums;
using System.Linq;

namespace spotify.companion.Model
{
    internal class Playlist : ItemBase
    {
        public Playlist() { }

        public Playlist(string id, ItemType itemType, string displayName,
            string uri, string imageSource, bool isSelected)
            :base(id, itemType, displayName, uri, imageSource, isSelected)
        {

        }

        public static Playlist Convert(SpotifyAPI.Web.SimplePlaylist item)
        {
            if (item == null) return null;

            try
            {
                string imgSource = (item.Images != null && item.Images.FirstOrDefault() != null) ?
                item.Images.FirstOrDefault().Url : "";

                int count = 0;
                if (item.Tracks != null && item.Tracks.Total.HasValue) count = item.Tracks.Total.Value;

                return new Playlist
                {
                    Id = item.Id,
                    ItemType = ItemType.Playlist,
                    DisplayName = item.Name,
                    Description = item.Description,
                    Uri = item.Uri,
                    ImageSource = imgSource,
                    Count = count,
                    Owner = new(item.Owner?.Id,
                                ItemType.User,
                                item.Owner?.DisplayName,
                                item.Owner?.Uri,
                                item.Owner?.Images?.FirstOrDefault()?.Url,
                                false),
                };
            }
            catch (System.Exception)
            {
                return null;
            }

            
        }

        public static Playlist Convert(SpotifyAPI.Web.FullPlaylist item)
        {
            if (item == null) return null;

            string imgSource = (item.Images != null && item.Images.FirstOrDefault() != null) ?
                item.Images.FirstOrDefault().Url : "";

            int count = 0;
            if (item.Tracks != null && item.Tracks.Total.HasValue) count = item.Tracks.Total.Value;

            return new Playlist
            {
                Id = item.Id,
                ItemType = ItemType.Playlist,
                DisplayName = item.Name,
                Description = item.Description,
                Uri = item.Uri,
                ImageSource = imgSource,
                Count = count,
                Owner = new(item.Owner?.Id,
                            ItemType.User,
                            item.Owner?.DisplayName,
                            item.Owner?.Uri,
                            item.Owner?.Images?.FirstOrDefault()?.Url,
                            false),
            };
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private User _owner;
        public User Owner
        {
            get => _owner;
            set => SetProperty(ref _owner, value);
        }

        public string Creator 
        {
            get => this.Owner?.DisplayName;
        }

        private bool _canModify;
        public bool CanModify
        {
            get => _canModify;
            set => SetProperty(ref _canModify, value);
        }

        private int _count;
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }
    }
}
