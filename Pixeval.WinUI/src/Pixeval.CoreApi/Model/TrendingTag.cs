using JetBrains.Annotations;

namespace Pixeval.CoreApi.Model
{
    [PublicAPI]
    public record TrendingTag
    {
        public string? Tag { get; set; }
        
        public string? Translation { get; set; }
        
        public Illustration? Illustration { get; set; }
    }
}