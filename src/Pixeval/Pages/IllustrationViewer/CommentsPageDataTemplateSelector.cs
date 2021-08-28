using Mako.Net.Response;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Pages.IllustrationViewer
{
    public class CommentsPageDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? TextCommentTemplate { get; set; }

        public DataTemplate? StickerCommentTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(object item)
        {
            return (item as IllustrationCommentsResponse.Comment)?.Stamp is null
                ? TextCommentTemplate
                : StickerCommentTemplate;
        }
    }
}
