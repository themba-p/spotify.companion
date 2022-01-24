namespace spotify.companion.Services
{
    // Navigation interface
    public interface INavigationService
    {
        bool CanGoBack { get; }
        void GoBack();
        void Navigate<T>(object args = null);
    }
}
