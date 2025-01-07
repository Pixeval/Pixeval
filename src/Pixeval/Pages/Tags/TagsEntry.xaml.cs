// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Frozen;
using System.IO;
using Windows.Storage;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls;
using Pixeval.Controls.DialogContent;
using Pixeval.Controls.Windowing;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Tags;

[DependencyProperty<TagsEntryViewModel>("ViewModel")]
public sealed partial class TagsEntry
{
    public event Action<TagsEntry, string>? TagClick;

    public event Action<TagsEntry, TagsEntryViewModel>? FileDeleted;

    public TagsEntry() => InitializeComponent();

    private void TagButton_OnClicked(object sender, ItemClickEventArgs e) => TagClick?.Invoke(this, e.ClickedItem.To<string>());

    private async void GoToPageItem_OnClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Illustration is null)
        {
            var hWnd = WindowFactory.GetWindowForElement(this).HWnd;
            var growl = hWnd.InfoGrowlReturn(TagsEntryResources.FetchingWorkInfo);
            try
            {
                var illustration = await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(ViewModel.Id);
                ViewModel.Illustration = illustration;
                if (growl is not null)
                    Growl.RemoveGrowl(hWnd, growl);
            }
            catch (Exception exception)
            {
                if (growl is null)
                    return;
                growl.Severity = InfoBarSeverity.Error;
                growl.Title = TagsEntryResources.FetchingWorkInfoFailed;
                growl.Message = exception.Message;
                return;
            }
        }
        var vm = new IllustrationItemViewModel(ViewModel.Illustration);
        vm.CreateWindowWithPage([vm]);
    }

    private async void DeleteItem_OnClicked(object sender, RoutedEventArgs e)
    {
        var file = await StorageFile.GetFileFromPathAsync(ViewModel.FullPath);
        await file.DeleteAsync();
        FileDeleted?.Invoke(this, ViewModel);
    }

    private async void OpenItem_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(ViewModel.FullPath));
    }

    private async void OpenLocationItem_OnClicked(object sender, RoutedEventArgs e)
    {
        _ = await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(ViewModel.FullPath));
    }

    private async void EditTagItem_OnClicked(object sender, RoutedEventArgs e)
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
