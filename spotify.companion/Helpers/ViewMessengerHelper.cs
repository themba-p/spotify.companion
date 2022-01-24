using spotify.companion.Enums;

namespace spotify.companion.Helpers
{
    internal class ViewMessengerHelper
    {
        public ViewMessengerHelper(ViewHelperType type, object payload = null)
        {
            this.Type = type;
            this.Payload = payload;
        }

        public ViewHelperType Type { get; set; }
        public object Payload { get; set; }
    }
}
