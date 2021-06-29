using JetBrains.Annotations;

namespace Pixeval.CoreApi
{
    [PublicAPI]
    public interface IMakoClientSupport
    {
        public MakoClient MakoClient { get; }
    }
}