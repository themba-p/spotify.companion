using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using spotify.companion.Broker;
using spotify.companion.Enums;
using spotify.companion.Helpers;
using spotify.companion.Services;
using System.Threading.Tasks;

namespace spotify.companion.ViewModel
{
    internal class StartPageViewModel : ObservableObject
    {
        private readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();
        private readonly INavigationService NavigationService;

        public StartPageViewModel(INavigationService navigationService)
        {
            this.NavigationService = navigationService;
            RetryCommand = new(Login);
            Login();
        }

        private async void Login()
        {
            IsLoading = true;

            if (await Helpers.Helpers.IsConnectedToInternet())
            {
                IsConnected = true;
                LoginMessengerHelper loginResponse = await AuthBroker.LoginAsync();
                await HandleLoginResponse(loginResponse);
            }
            else
            {
                IsConnected = false;
            }

            IsLoading = false;
        }

        private async Task<bool> HandleLoginResponse(LoginMessengerHelper loginResponse)
        {
            if (loginResponse != null && loginResponse.ResponseType == ResponseType.Success)
            {
                DataBroker.InitializeClient(loginResponse.Payload as SpotifyAPI.Web.SpotifyClient);
                NavigationService.Navigate<MainPageViewModel>();
                WeakReferenceMessenger.Default.Send(loginResponse.CurrentUser);
            } else
            {
                if (dispatcher != null)
                {
                    dispatcher.TryEnqueue(() =>
                    {
                        NavigationService.Navigate<LoginPageViewModel>();
                    });
                }
                else
                {
                    await Task.Delay(250);
                    NavigationService.Navigate<LoginPageViewModel>();
                }

            }

            return true;
        }

        public RelayCommand RetryCommand { get; }

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
