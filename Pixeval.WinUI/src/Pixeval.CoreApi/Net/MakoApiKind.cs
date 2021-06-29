using JetBrains.Annotations;

namespace Pixeval.CoreApi.Net
{
    /// <summary>
    /// Mako所使用到的不同API类型
    /// </summary>
    [PublicAPI]
    public enum MakoApiKind
    {
        AppApi, WebApi, AuthApi, ImageApi
    }
}