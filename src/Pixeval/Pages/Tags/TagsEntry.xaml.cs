using System;
using System.Collections.Frozen;
using System.IO;
using Windows.Storage;
using Windows.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls;
using Pixeval.Controls.DialogContent;
using Pixeval.Logging;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Tags;

[DependencyProperty<TagsEntryViewModel>("ViewModel")]
public sealed partial class TagsEntry : IViewModelControl
{
    object IViewModelControl.ViewModel => ViewModel;

    public event Action<TagsEntry, string>? TagTapped;

    public event Action<TagsEntry, TagsEntryViewModel>? FileDeleted;

    public TagsEntry() => InitializeComponent();

    private void TagButton_OnTapped(object sender, TappedRoutedEventArgs e) => TagTapped?.Invoke(this, sender.To<Button>().Content.To<string>());

    private void GoToPageItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var vm = new IllustrationItemViewModel(ViewModel.Illustration!);
        vm.CreateWindowWithPage([vm]);
    }

    private async void DeleteItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var file = await StorageFile.GetFileFromPathAsync(ViewModel.FullPath);
        await file.DeleteAsync();
        FileDeleted?.Invoke(this, ViewModel);
    }

    private async void OpenItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(ViewModel.FullPath));
    }

    private async void OpenLocationItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(ViewModel.FullPath));
    }

    private async void EditTagItem_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var content = new TagsEntryEditTagDialog(ViewModel);

        if (await this.CreateOkCancelAsync(TagsEntryResources.EditTag, content) is ContentDialogResult.Primary)
        {
            ViewModel.TagsSet = content.Tags.ToFrozenSet();
            if (await ViewModel.SaveTagsAsync() is { } errorMessage)
            {
                _ = await this.CreateAcknowledgementAsync(TagsEntryResources.EditTagFailed, errorMessage);
            }
        }
    }
}
