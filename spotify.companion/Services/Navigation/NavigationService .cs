using Microsoft.UI.Xaml.Controls;
using spotify.companion.View;
using spotify.companion.ViewModel;
using System;
using System.Collections.Generic;

namespace spotify.companion.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<Type, Type> viewMapping = new()
        {
            [typeof(StartPageViewModel)] = typeof(StartPage),
            [typeof(LoginPageViewModel)] = typeof(LoginPage),
            [typeof(MainPageViewModel)] = typeof(MainPage),
        };

        private readonly Frame NavigationFrame;     

        public NavigationService(Frame navigationFrame) 
        {
            this.NavigationFrame = navigationFrame;
        }

        public bool CanGoBack => this.NavigationFrame.CanGoBack;

        public void GoBack() => this.NavigationFrame.GoBack();

        public void Navigate<T>(object args = null)
        {
            this.NavigationFrame.Navigate(this.viewMapping[typeof(T)], args);
        }
    }

}
