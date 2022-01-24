using spotify.companion.Enums;
using System.Linq;

namespace spotify.companion.Model
{
    internal class User : ItemBase
    {
        public User() { }

        public User(string id, ItemType itemType, string displayName, 
            string uri, string imageSource, bool isSelected)
            : base(id, itemType, displayName, uri, imageSource, isSelected)
        {

        }

        public static User Convert(SpotifyAPI.Web.PrivateUser user)
        {
            if (user == null) return null;

            string imgSource = (user.Images != null && user.Images.FirstOrDefault() != null) ?
                user.Images.FirstOrDefault().Url : "";
            
            return new User
            {
                Id = user.Id,
                ItemType = ItemType.User,
                DisplayName = user.DisplayName,
                Uri = user.Uri,
                ImageSource = imgSource,
                Email = user.Email,
                Product = user.Product,
            };
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _product;
        public string Product
        {
            get => _product;
            set => SetProperty(ref _product, value);
        }
    }
}
