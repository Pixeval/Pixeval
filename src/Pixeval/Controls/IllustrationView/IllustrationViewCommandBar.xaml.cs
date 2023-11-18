#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewCommandBar.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.CoreApi.Model;
using Pixeval.Download;
using Pixeval.Flyouts.IllustrationResultFilter;
using Pixeval.Controls.TokenInput;
using Pixeval.Util;
using Pixeval.Util.Threading;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Windows.System;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.IllustrationView;

[DependencyProperty<ObservableCollection<ICommandBarElement>>("PrimaryCommandsSupplements", DependencyPropertyDefaultValue.New, nameof(OnPrimaryCommandsSupplementsChanged))]
[DependencyProperty<ObservableCollection<ICommandBarElement>>("SecondaryCommandsSupplements", DependencyPropertyDefaultValue.New, nameof(OnSecondaryCommandsSupplementsChanged))]
[DependencyProperty<bool>("IsDefaultCommandsEnabled", "true", nameof(OnIsDefaultCommandsEnabledChanged))]
[DependencyProperty<IllustrationViewViewModel>("ViewModel")]
[DependencyProperty<bool>("ShowDefaultSuggestBox", "true")]
public sealed partial class IllustrationViewCommandBar
{
    private readonly IEnumerable<Control> _defaultCommands;

    private FilterSettings _lastFilterSettings = FilterSettings.Default;

    public IllustrationViewCommandBar()
    {
        InitializeComponent();
        var defaultCommands = new List<ICommandBarElement>(CommandBar.PrimaryCommands);
        defaultCommands.AddRange(CommandBar.SecondaryCommands);
        _defaultCommands = defaultCommands.Where(e => e is AppBarButton).Cast<Control>();
        CommandBarElements = [];
        CommandBarElements.CollectionChanged += (_, args) =>
        {
            switch (args)
            {
                case { Action: NotifyCollectionChangedAction.Add }:
                    if (args is { NewItems: not null })
                    {
                        foreach (UIElement argsNewItem in args.NewItems)
                        {
                            ExtraCommandsBar.Children.Insert(ExtraCommandsBar.Children.Count - 1, argsNewItem);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
    }

    public ObservableCollection<UIElement> CommandBarElements { get; }

    private static void OnPrimaryCommandsSupplementsChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        AddCommandCallback(args, ((IllustrationViewCommandBar)o).CommandBar.PrimaryCommands);
    }

    private static void OnSecondaryCommandsSupplementsChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        AddCommandCallback(args, ((IllustrationViewCommandBar)o).CommandBar.SecondaryCommands);
    }

    private static void OnIsDefaultCommandsEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        ((IllustrationViewCommandBar)o)._defaultCommands
            .Where(c => c != ((IllustrationViewCommandBar)o).SelectAllButton)
            .ForEach(c => c.IsEnabled = (bool)args.NewValue);
    }

    private static void AddCommandCallback(DependencyPropertyChangedEventArgs e, ICollection<ICommandBarElement> commands)
    {
        if (e.NewValue is ObservableCollection<ICommandBarElement> collection)
        {
            collection.CollectionChanged += (_, args) =>
            {
                switch (args)
                {
                    case { Action: NotifyCollectionChangedAction.Add }:
                        foreach (var argsNewItem in args.NewItems ?? Array.Empty<ICommandBarElement>())
                        {
                            commands.Add((ICommandBarElement)argsNewItem);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(e), @"This collection does not support operations except the Add");
                }
            };
        }
    }

    private void SelectAllToggleButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.DataProvider.Source.WhereNotNull().ForEach(i => i.IsSelected = true);
    }

    private async void AddAllToBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        // TODO custom bookmark tag
        var notBookmarked = ViewModel.DataProvider.SelectedIllustrations.Where(i => !i.IsBookmarked);
        var viewModelSelectedIllustrations = notBookmarked as IllustrationViewModel[] ?? notBookmarked.ToArray();
        if (viewModelSelectedIllustrations.Length > 5 && await MessageDialogBuilder.CreateOkCancel(
                    this,
                    IllustrationViewCommandBarResources.SelectedTooManyItemsForBookmarkTitle,
                    IllustrationViewCommandBarResources.SelectedTooManyItemsForBookmarkContent)
                .ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        foreach (var viewModelSelectedIllustration in viewModelSelectedIllustrations)
        {
            viewModelSelectedIllustration.PostPublicBookmarkAsync().Discard(); // discard the result
        }

        if (viewModelSelectedIllustrations.Length is var c and > 0)
        {
            _ = MessageDialogBuilder.CreateAcknowledgement(this,
                IllustrationViewCommandBarResources.AddAllToBookmarkTitle,
                IllustrationViewCommandBarResources.AddAllToBookmarkContentFormatted.Format(c));
        }
    }

    private async void SaveAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel.DataProvider.SelectedIllustrations.Count >= 20 && await MessageDialogBuilder.CreateOkCancel(
                this,
                IllustrationViewCommandBarResources.SelectedTooManyItemsTitle,
                IllustrationViewCommandBarResources.SelectedTooManyItemsForSaveContent).ShowAsync()
            != ContentDialogResult.Primary)
        {
            return;
        }

        using var scope = App.AppViewModel.AppServicesScope;
        var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>>();

        // This will run for quite a while
        _ = Task.Run(async () =>
        {
            var tasks = await CurrentContext.Window.DispatcherQueue.EnqueueAsync(
                async () => await Task.WhenAll(
                    ViewModel.DataProvider.SelectedIllustrations
                        .SelectMany(i => i.GetMangaIllustrationViewModels())
                        .Select(i => factory.CreateAsync(i, App.AppViewModel.AppSetting.DefaultDownloadPathMacro))));
            foreach (var viewModelSelectedIllustration in tasks)
            {
                App.AppViewModel.DownloadManager.QueueTask(viewModelSelectedIllustration);
            }
        });

        CommandBarTeachingTip.ShowAndHide(IllustrationViewCommandBarResources.DownloadItemsQueuedFormatted.Format(ViewModel.DataProvider.SelectedIllustrations.Count), mSec: 1500);
    }

    private async void OpenAllInBrowserButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel.DataProvider.SelectedIllustrations is { Count: var count } selected)
        {
            if (count > 15 && await MessageDialogBuilder.CreateOkCancel(
                        this,
                        IllustrationViewCommandBarResources.SelectedTooManyItemsTitle,
                        IllustrationViewCommandBarResources.SelectedTooManyItemsForOpenInBrowserContent)
                    .ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            foreach (var illustrationViewModel in selected)
            {
                _ = await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(illustrationViewModel.Id));
            }
        }
    }

    private void ShareButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        // TODO
        throw new NotImplementedException();
    }

    private void CancelSelectionButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.DataProvider.Source.WhereNotNull().ForEach(i => i.IsSelected = false);
    }

    private void OpenConditionDialogButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        FilterTeachingTip.IsOpen = true;
    }

    private void FilterTeachingTip_OnActionButtonClick(TeachingTip sender, object args)
    {
        FilterContent.Reset();
    }

    private void FilterTeachingTip_OnCloseButtonClick(TeachingTip sender, object args)
    {
        if (FilterContent.GetFilterSettings is not (
            var includeTags,
            var excludeTags,
            var leastBookmark,
            var maximumBookmark,
            _, // TODO user group name
            var illustratorName,
            var illustratorId,
            var illustrationName,
            var illustrationId,
            var publishDateStart,
            var publishDateEnd) filterSettings)
            return;
        if (filterSettings == _lastFilterSettings)
        {
            return;
        }

        _lastFilterSettings = filterSettings;

        ViewModel.DataProvider.Filter = null;
        ViewModel.DataProvider.Filter = o =>
        {
            if (o is IllustrationViewModel vm)
            {
                var stringTags = vm.Illustrate.Tags?.Select(t => t.Name).WhereNotNull().ToArray() ??
                                 [];
                var result = ExamineExcludeTags(stringTags, excludeTags)
                             && ExamineIncludeTags(stringTags, includeTags)
                             && vm.Bookmark >= leastBookmark
                             && vm.Bookmark <= maximumBookmark
                             && illustrationName.Match(vm.Illustrate.Title)
                             && illustratorName.Match(vm.Illustrate.User?.Name)
                             && (illustratorId.IsNullOrEmpty() ||
                                 illustratorId == vm.Illustrate.User?.Id.ToString())
                             && (illustrationId.IsNullOrEmpty() || illustrationId == vm.Id)
                             && vm.PublishDate >= publishDateStart
                             && vm.PublishDate <= publishDateEnd;
                return result;
            }

            return false;
        };

        static bool ExamineExcludeTags(IEnumerable<string> tags, IEnumerable<Token> predicates)
        {
            return predicates.Aggregate(true, (acc, token) => acc && tags.None(token.Match));
        }

        static bool ExamineIncludeTags(IEnumerable<string> tags, IEnumerable<Token> predicates)
        {
            var tArr = tags.ToArray();
            return !tArr.Any() || predicates.Aggregate(true, (acc, token) => acc && tArr.Any(token.Match));
        }
    }

    private void FastFilterAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        PerformSearch(sender.Text);
    }

    public void PerformSearch(string text)
    {
        if (text.IsNullOrBlank())
        {
            ViewModel.DataProvider.Filter = null;
        }
        else
        {
            ViewModel.DataProvider.Filter = o =>
            {
                if (o is IllustrationViewModel viewModel)
                {
                    return viewModel.Id.Contains(text)
                           || (viewModel.Illustrate.Tags ?? Enumerable.Empty<Tag>()).Any(x => x.Name.Contains(text) || (x.TranslatedName?.Contains(text) ?? false))
                           || (viewModel.Illustrate.Title?.Contains(text) ?? false);
                }

                return false;
            };
        }
    }
}
