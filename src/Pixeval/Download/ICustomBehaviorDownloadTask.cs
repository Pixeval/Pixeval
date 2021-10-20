using Windows.Storage.Streams;

namespace Pixeval.Download
{
    public interface ICustomBehaviorDownloadTask : IDownloadTask
    {
        void Consume(IRandomAccessStream stream);
    }
}