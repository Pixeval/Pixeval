using JetBrains.Annotations;

namespace Pixeval.CoreApi.Model
{
    [PublicAPI]
    public record Tag
    {
        public string? Name { get; init; }

        public string? TranslatedName { get; init; }
    }
}