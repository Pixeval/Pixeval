using System.Collections.Generic;
using Pixeval.Download.MacroParser;

namespace Pixeval.Download
{
    public interface IDownloadTaskFactory<T>
    {
        IMetaPathParser<T> PathParser { get; }

        IEnumerable<IDownloadTask> Create(T context, string rawPath);
    }
}