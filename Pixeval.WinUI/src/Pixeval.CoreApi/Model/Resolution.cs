using JetBrains.Annotations;

namespace Pixeval.CoreApi.Model
{
    [PublicAPI]
    public record Resolution
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}