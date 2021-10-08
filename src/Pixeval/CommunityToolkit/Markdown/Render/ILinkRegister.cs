// ReSharper disable IdentifierTypo

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace Pixeval.CommunityToolkit.Markdown.Render
{
    /// <summary>
    /// An interface used to handle links in the markdown.
    /// </summary>
    public interface ILinkRegister
    {
        /// <summary>
        /// Registers a Hyperlink with a LinkUrl.
        /// </summary>
        /// <param name="newHyperlink">Hyperlink to Register.</param>
        /// <param name="linkUrl">Url to Register.</param>
        void RegisterNewHyperLink(Hyperlink newHyperlink, string linkUrl);

        /// <summary>
        /// Registers a Hyperlink with a LinkUrl.
        /// </summary>
        /// <param name="newImagelink">ImageLink to Register.</param>
        /// <param name="linkUrl">Url to Register.</param>
        /// <param name="isHyperLink">Is Image an IsHyperlink.</param>
        void RegisterNewHyperLink(Image newImagelink, string linkUrl, bool isHyperLink);
    }
}