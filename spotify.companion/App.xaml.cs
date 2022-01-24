using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using spotify.companion.Model;
using spotify.companion.Services;
using spotify.companion.View;
using spotify.companion.ViewModel;
using System;
using System.Runtime.InteropServices;
using WinRT;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace spotify.companion
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }  

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new ShellWindow();

            //Get the Window's HWND
            var windowNative = m_window.As<IWindowNative>();
            m_windowHandle = windowNative.WindowHandle;
            m_window.Title = "Spotify Companion";
            SetWindowSize(m_windowHandle, 380, 600);

                      

            Ioc.Default.ConfigureServices(ConfigureServices());
            Ioc.Default.GetService<INavigationService>().Navigate<StartPageViewModel>();
            m_window.Activate();
        }

        private IServiceProvider ConfigureServices()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton<INavigationService, NavigationService>();
            serviceCollection.AddScoped<INavigationService>(sp =>
            {
                return new NavigationService((m_window as ShellWindow).NavigationFrame);
            });

            serviceCollection.AddSingleton<StartPage>();
            serviceCollection.AddSingleton<StartPageViewModel>();

            serviceCollection.AddSingleton<LoginPage>();
            serviceCollection.AddSingleton<LoginPageViewModel>();

            serviceCollection.AddSingleton<MainPage>();
            serviceCollection.AddSingleton<MainPageViewModel>();

            return serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// The Window object doesn't have Width and Height properties in WInUI 3 Desktop yet.
        /// To set the Width and Height, you can use the Win32 API SetWindowPos.
        /// Note, you should apply the DPI scale factor if you are thinking of dpi instead of pixels.
        /// </summary>
        private static void SetWindowSize(IntPtr hwnd, int width, int height)
        {
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE);
        }

        private Window m_window;
        private IntPtr m_windowHandle;
        public IntPtr WindowHandle { get { return m_windowHandle; } }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
    }
}
