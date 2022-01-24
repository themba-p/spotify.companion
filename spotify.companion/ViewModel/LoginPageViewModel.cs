using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using spotify.companion.Broker;
using spotify.companion.Enums;
using spotify.companion.Helpers;
using spotify.companion.Model;
using spotify.companion.Services;
using System.Threading.Tasks;

namespace spotify.companion.ViewModel
{
    internal class LoginPageViewModel : ObservableObject
    {
        private readonly INavigationService NavigationService;
        private readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();

        public LoginPageViewModel(INavigationService navigationService)
        {
            this.NavigationService = navigationService;
            LoginCommand = new AsyncRelayCommand(Authenticate);
            RetryCommand = new(Authenticate);
            WeakReferenceMessenger.Default.Register<LoginMessengerHelper>(this, (r, m) =>
            {
                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(() =>
                    {
                        HandleLoginResponse(m);
                    });
                }
                
            });
        }

        private async Task Authenticate()
        {
            IsLoading = true;

            if (await Helpers.Helpers.IsConnectedToInternet())
            {
                IsConnected = true;
                await AuthBroker.AuthenticateAsync();
            } else
            {
                IsLoading = false;
                IsConnected = false;
            }
        }

        private void HandleLoginResponse(LoginMessengerHelper loginResponse)
        {
            string title;
            string notifyMessage;
            bool autoDismiss = false;

            if (loginResponse == null)
            {
                title = "";
                notifyMessage = "Unknown error, please try again";
            }
            else
            {

                notifyMessage = loginResponse.ResponseType switch
                {
                    ResponseType.Success => "Authentication successful",
                    ResponseType.Cancelled => "Error, Authentication cancelled",
                    _ => "Unknown error, please try again",
                };

                autoDismiss = loginResponse.ResponseType switch
                {
                    ResponseType.Success => true,
                    ResponseType.Cancelled => false,
                    _ => false,
                };

                title = loginResponse.ResponseType switch
                {
                    ResponseType.Success => "Success",
                    ResponseType.Cancelled => "Cancelled",
                    _ => "Error",
                };
            }

            InAppNotification notification = new(loginResponse.ResponseType, notifyMessage, title, autoDismiss);
            WeakReferenceMessenger.Default.Send(notification);

            if (loginResponse != null && loginResponse.ResponseType == ResponseType.Success)
            {
                DataBroker.InitializeClient(loginResponse.Payload as SpotifyAPI.Web.SpotifyClient);
                NavigationService.Navigate<MainPageViewModel>();
                WeakReferenceMessenger.Default.Send(loginResponse.CurrentUser);
            }

            IsLoading = false;
        }

        public AsyncRelayCommand LoginCommand { get; }
        public AsyncRelayCommand RetryCommand { get; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isConnected = true;
        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }
    }

}
