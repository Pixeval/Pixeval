namespace Pixeval.Navigation
{
    public interface INavigablePage
    {
        void OnNavigatedFrom(INavigationRoot root, INavigablePage? from, params object?[]? args);
    }
}
