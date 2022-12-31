namespace Pixeval.Navigation
{
    public interface INavigablePage
    {
        void OnNavigatingToAsync(INavigationRoot root, INavigablePage? from, params object?[]? args);
        void OnNavigatedToAsync(INavigationRoot root, INavigablePage? from, params object?[]? args);
        void OnNavigatingFromAsync(INavigationRoot root, INavigablePage? from, params object?[]? args);
        void OnNavigatedFromAsync(INavigationRoot root, INavigablePage? from, params object?[]? args);
    }
}
