using JetBrains.Annotations;

namespace Pixeval.CoreApi.Engines
{
    [PublicAPI]
    public interface IEngineHandleSource
    {
        EngineHandle EngineHandle { get; }
    }
}