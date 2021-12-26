#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationGridCommandBar.xaml.cs
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
using Windows.System;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Attributes;
using Pixeval.Download;

using Pixeval.Popups;
using Pixeval.Popups.IllustrationResultFilter;
using Pixeval.UserControls.TokenInput;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.UserControls;

[DependencyProperty("PrimaryCommandsSupplements", typeof(ObservableCollection<ICommandBarElement>), nameof(OnPrimaryCommandsSupplementsChanged), DefaultValue = "new ObservableCollection<ICommandBarElement>()")]
[DependencyProperty("SecondaryCommandsSupplements", typeof(ObservableCollection<ICommandBarElement>), nameof(OnSecondaryCommandsSupplementsChanged), DefaultValue = "new ObservableCollection<ICommandBarElement>()")]
[DependencyProperty("IsDefaultCommandsEnabled", typeof(bool), nameof(OnIsDefaultCommandsEnabledChanged), DefaultValue = "true")]
[DependencyProperty("ViewModel", typeof(IllustrationGridViewModel), DefaultValue = "new IllustrationGridViewModel()")]
public sealed partial class IllustrationGridCommandBar
{
    private readonly IEnumerable<Control> _defaultCommands;

    private readonly IllustrationResultFilterPopupViewModel _filterPopupViewModel = new();

    public IllustrationGridCommandBar()
    {
        InitializeComponent();
        var defaultCommands = new List<ICommandBarElement>(CommandBar.PrimaryCommands);
        defaultCommands.AddRange(CommandBar.SecondaryCommands);
        _defaultCommands = defaultCommands.Where(e => e is AppBarButton).Cast<Control>();
        CommandBarElements = new ObservableCollection<UIElement>();
        CommandBarElements.CollectionChanged += (_, args) =>
        {
            switch (args)
            {
                case { Action: NotifyCollectionChangedAction.Add }:
                    if (args is { NewItems: not null })
                    {
                        foreach (UIElement argsNewItem in args.NewItems)
                        {
                            ExtraCommandsBar.Children.Add(argsNewItem);
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
        AddCommandCallback(args, ((IllustrationGridCommandBar) o).CommandBar.PrimaryCommands);
    }

    private static void OnSecondaryCommandsSupplementsChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        AddCommandCallback(args, ((IllustrationGridCommandBar) o).CommandBar.SecondaryCommands);
    }

    private static void OnIsDefaultCommandsEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        ((IllustrationGridCommandBar) o)._defaultCommands
            .Where(c => c != ((IllustrationGridCommandBar) o).SelectAllButton)
            .ForEach(c => c.IsEnabled = (bool) args.NewValue);
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
                            commands.Add((ICommandBarElement) argsNewItem);
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
        ViewModel.IllustrationsView.OfType<IllustrationViewModel>().ForEach(v => v.IsSelected = true);
    }

    private async void AddAllToBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        // TODO custom bookmark tag
        var notBookmarked = ViewModel.SelectedIllustrations.Where(i => !i.IsBookmarked);
        var viewModelSelectedIllustrations = notBookmarked as IllustrationViewModel[] ?? notBookmarked.ToArray();
        if (viewModelSelectedIllustrations.Length > 5 && await MessageDialogBuilder.CreateOkCancel(
                    this,
                    IllustrationGridCommandBarResources.SelectedTooManyItemsForBookmarkTitle,
                    IllustrationGridCommandBarResources.SelectedTooManyItemsForBookmarkContent)
                .ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        foreach (var viewModelSelectedIllustration in viewModelSelectedIllustrations)
        {
            _ = viewModelSelectedIllustration.PostPublicBookmarkAsync(); // discard the result
        }

        if (viewModelSelectedIllustrations.Length is var c and > 0)
        {
            MessageDialogBuilder.CreateAcknowledgement(this,
                IllustrationGridCommandBarResources.AddAllToBookmarkTitle,
                IllustrationGridCommandBarResources.AddAllToBookmarkContentFormatted.Format(c));
        }
    }

    private async void SaveAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedIllustrations.Count >= 20 && await MessageDialogBuilder.CreateOkCancel(
                this,
                IllustrationGridCommandBarResources.SelectedTooManyItemsTitle,
                IllustrationGridCommandBarResources.SelectedTooManyItemsForSaveContent).ShowAsync()
            != ContentDialogResult.Primary)
        {
            return;
        }

        using var scope = App.AppViewModel.AppServicesScope;
        var factory = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationViewModel, ObservableDownloadTask>>();

        // This will run for quite a while
        _ = Task.Run(async () =>
        {
            var tasks = await App.AppViewModel.Window.DispatcherQueue.EnqueueAsync(
                async () => await Task.WhenAll(
                    ViewModel.SelectedIllustrations
                        .SelectMany(i => i.GetMangaIllustrationViewModels())
                        .Select(i => factory.CreateAsync(i, App.AppViewModel.AppSetting.DefaultDownloadPathMacro))));
            foreach (var viewModelSelectedIllustration in tasks)
            {
                App.AppViewModel.DownloadManager.QueueTask(viewModelSelectedIllustration);
            }
        });
    }

    private async void OpenAllInBrowserButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedIllustrations is { Count: var count } selected)
        {
            if (count > 15 && await MessageDialogBuilder.CreateOkCancel(
                        this,
                        IllustrationGridCommandBarResources.SelectedTooManyItemsTitle,
                        IllustrationGridCommandBarResources.SelectedTooManyItemsForOpenInBrowserContent)
                    .ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            foreach (var illustrationViewModel in selected)
            {
                await Launcher.LaunchUriAsync(MakoHelper.GenerateIllustrationWebUri(illustrationViewModel.Id));
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
        ViewModel.Illustrations.ForEach(v => v.IsSelected = false);
    }

    private void OpenConditionDialogButton_OnChecked(object sender, RoutedEventArgs e)
    {
        var content = new IllustrationResultFilterPopupContent(_filterPopupViewModel);
        content.ResetButtonTapped += (_, _) => ViewModel.IllustrationsView.Filter = null;
        var popup = PopupManager.CreatePopup(content, 550, heightMargin: 100, lightDismiss: false, closing: ConditionPopupClosing);
        content.CloseButtonTapped += (_, _) =>
        {
            content.Cleanup();
            PopupManager.ClosePopup(popup);
        };
        PopupManager.ShowPopup(popup);
    }

    private void ConditionPopupClosing(IAppPopupContent popup, object? arg)
    {
        OpenConditionDialogButton.IsChecked = false;
        if (arg is FilterSettings(
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
                var publishDateEnd)
            && popup is IllustrationResultFilterPopupContent { IsReset: false })
        {
            ViewModel.IllustrationsView.Filter = null;
            ViewModel.IllustrationsView.Filter += o =>
            {
                if (o is IllustrationViewModel vm)
                {
                    var stringTags = vm.Illustration.Tags?.Select(t => t.Name).WhereNotNull().ToArray() ?? Array.Empty<string>();
                    var result = ExamineExcludeTags(stringTags, excludeTags)
                                 && ExamineIncludeTags(stringTags, includeTags)
                                 && vm.Bookmark >= leastBookmark
                                 && vm.Bookmark <= maximumBookmark
                                 && illustrationName.Match(vm.Illustration.Title)
                                 && illustratorName.Match(vm.Illustration.User?.Name)
                                 && (illustratorId.IsNullOrEmpty() || illustratorId == vm.Illustration.User?.Id.ToString())
                                 && (illustrationId.IsNullOrEmpty() || illustrationId == vm.Id)
                                 && vm.PublishDate >= publishDateStart
                                 && vm.PublishDate <= publishDateEnd;
                    return result;
                }

                return false;
            };
        }
        else if (popup is IllustrationResultFilterPopupContent { IsReset: true })
        {
            ViewModel.IllustrationsView.Refresh();
        }

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
}