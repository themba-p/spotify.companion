using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using spotify.companion.Enums;
using spotify.companion.Helpers;
using spotify.companion.Model;
using spotify.companion.Services;

namespace spotify.companion.ViewModel
{
    internal class ShellViewModel : ObservableObject
    {
        private readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();

        public ShellViewModel()
        {
            WeakReferenceMessenger.Default.Register<InAppNotification>(this, (r, m) =>
            {
                if (m != null)
                {
                    if (m.ResponseType == ResponseType.Hide)
                    {
                        if (dispatcher != null && Notification != null)
                        {
                            dispatcher.TryEnqueue(() =>
                            {
                                Notification.Hide();
                                Notification = null;
                            });
                        }
                        
                    }
                    else
                    {
                        if (dispatcher != null)
                        {
                            dispatcher.TryEnqueue(() =>
                            {
                                Notification = m;
                                Notification.Show();
                            });
                        }
                        
                    }
                }
            });
            WeakReferenceMessenger.Default.Register<NavMessengerHelper>(this, (r, m) =>
            {
                if (m != null)
                {
                    if (dispatcher != null && Notification != null)
                    {
                        dispatcher.TryEnqueue(() =>
                        {
                            if (m.NavTargetType == NavTargetType.Login)
                                Ioc.Default.GetService<INavigationService>().Navigate<LoginPageViewModel>();
                        });
                    }
                }
            });
        }

        private InAppNotification _notification;
        public InAppNotification Notification
        {
            get => _notification;
            private set => SetProperty(ref _notification, value);
        }
    }
}
