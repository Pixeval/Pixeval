using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Model;
using Pixeval.Models.Database.Managers;

namespace Pixeval.Views.Work;

public partial class NovelItem : WorkItem
{
    public NovelItem() => InitializeComponent();

    private void UIElement_OnClicked(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is not Control { DataContext: Tag tag })
            return;
        SearchHistoryPersistentManager.AddHistory(tag.Name, tag.TranslatedName);
        // TopLevel.GetTopLevel(this)?.ViewContainer?.NavigateTo<SearchWorksPage>(tag.Name, (SimpleWorkType.Novel, Name));
    }

    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        CoverImage.Opacity = 0;
        DescriptionBlock.Opacity = 1;
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        DescriptionBlock.Opacity = 0;
        CoverImage.Opacity = 1;
    }
}
