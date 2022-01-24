using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace spotify.companion.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellWindow : Window
    {
        private readonly OverlappedPresenter _presenter;

        public ShellWindow()
        {
            this.InitializeComponent();

            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(CustomTitleBar);

            _presenter = GetApplicationWindowPresenter();
            _presenter.IsMaximizable = false;
            _presenter.IsResizable = false;           
        }

        public OverlappedPresenter GetApplicationWindowPresenter()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
             WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow _apw = AppWindow.GetFromWindowId(myWndId); 
            return _apw.Presenter as OverlappedPresenter;
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            _presenter?.Minimize();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
