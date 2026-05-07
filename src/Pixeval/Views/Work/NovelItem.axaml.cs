using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.Views.Capability;

namespace Pixeval.Views.Work;

public partial class NovelItem : WorkItem
{
    public NovelItem() => InitializeComponent();

    private void TagButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: Tag tag })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
            return;
        App.AppViewModel.HistoryPersistHelper.AddSearchHistory(tag.Name, tag.TranslatedName);
        viewContainer.NavigateTo(new SearchWorksPage(tag.Name, SimpleWorkType.Novel));
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
