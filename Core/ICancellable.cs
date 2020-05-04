namespace Pixeval.Core
{
    public interface ICancellable
    {
        void Cancel();

        bool IsCancellationRequested();
    }
}