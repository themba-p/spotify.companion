using Microsoft.Toolkit.Mvvm.ComponentModel;
using spotify.companion.Enums;

namespace spotify.companion.Model
{
    internal class Response : ObservableObject
    {
        private ResponseType _responseType;
        public ResponseType ResponseType 
        { 
            get => _responseType;
            set => SetProperty(ref _responseType, value);  
        }

        private object _payload;
        public object Payload
        {
            get => _payload;
            set => SetProperty(ref _payload, value);
        }

        public Response(ResponseType responseType, object payload)
        {
            ResponseType = responseType;
            Payload = payload;
        }
    }
}
