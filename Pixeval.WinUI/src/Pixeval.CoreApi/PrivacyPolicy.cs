using JetBrains.Annotations;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi
{
    [PublicAPI]
    public enum PrivacyPolicy
    {
        [Description("public")]
        Public, 
        
        [Description("private")]
        Private
    }
}