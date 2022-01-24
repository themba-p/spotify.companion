using spotify.companion.Enums;
using spotify.companion.Model;

namespace spotify.companion.Helpers
{
    internal class LoginMessengerHelper : Response
    {
        public LoginMessengerHelper(ResponseType responseType, object payload, User currentUser)
            :base(responseType, payload)
        {
            CurrentUser = currentUser;
        }

        private User _currentUser;
        public User CurrentUser { get { return _currentUser; } set { _currentUser = value; } }
    }
}
