namespace Pixeval.Popups
{
    public interface ICompletableAppPopupContent : IAppPopupContent
    {
        object GetCompletionResult();
    }
}