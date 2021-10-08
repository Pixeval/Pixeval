using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    /// An interface used to resolve images in the markdown.
    /// </summary>
    public interface IImageResolver
    {
        /// <summary>
        /// Resolves an Image from a Url.
        /// </summary>
        /// <param name="url">Url to Resolve.</param>
        /// <param name="tooltip">Tooltip for Image.</param>
        /// <returns>Image</returns>
        Task<ImageSource?> ResolveImageAsync(string url, string tooltip);
    }
}