using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.System;
using Mako.Util;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    public sealed partial class IllustrationGridCommandBar
    {
        private readonly IEnumerable<Control> _defaultCommands;

        public IllustrationGridCommandBar()
        {
            InitializeComponent();
            var defaultCommands = new List<ICommandBarElement>(CommandBar.PrimaryCommands);
            defaultCommands.AddRange(CommandBar.SecondaryCommands);
            _defaultCommands = defaultCommands.Where(e => e is AppBarButton).Cast<Control>();
        }

        public static DependencyProperty PrimaryCommandSupplementsProperty = DependencyProperty.Register(
            nameof(PrimaryCommandsSupplements),
            typeof(ObservableCollection<ICommandBarElement>),
            typeof(IllustrationGridCommandBar),
            PropertyMetadata.Create(
                new ObservableCollection<ICommandBarElement>(),
                (o, args) => AddCommandCallback(args, ((IllustrationGridCommandBar) o).CommandBar.PrimaryCommands)));

        public static DependencyProperty SecondaryCommandsSupplementsProperty = DependencyProperty.Register(
            nameof(SecondaryCommandsSupplements),
            typeof(ObservableCollection<ICommandBarElement>),
            typeof(IllustrationGridCommandBar),
            PropertyMetadata.Create(
                new ObservableCollection<ICommandBarElement>(),
                (o, args) => AddCommandCallback(args, ((IllustrationGridCommandBar) o).CommandBar.SecondaryCommands)));

        public static DependencyProperty IsDefaultCommandsEnabledProperty = DependencyProperty.Register(
            nameof(IsDefaultCommandsEnabled),
            typeof(bool),
            typeof(IllustrationGridCommandBar),
            PropertyMetadata.Create(true,
                (o, args) => ((IllustrationGridCommandBar) o)._defaultCommands.ForEach(c => c.IsEnabled = (bool) args.NewValue)));

        public static DependencyProperty CommandBarContentProperty = DependencyProperty.Register(
            nameof(CommandBarContent),
            typeof(object),
            typeof(IllustrationGridCommandBar),
            PropertyMetadata.Create(new object(), (o, args) => ((IllustrationGridCommandBar) o).CommandBar.Content = args.NewValue));

        public static DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(IllustrationGridViewModel),
            typeof(IllustrationGridCommandBar),
            PropertyMetadata.Create(new IllustrationGridViewModel()));

        public ObservableCollection<ICommandBarElement> PrimaryCommandsSupplements
        {
            get => (ObservableCollection<ICommandBarElement>) GetValue(PrimaryCommandSupplementsProperty);
            set => SetValue(PrimaryCommandSupplementsProperty, value);
        }

        public ObservableCollection<ICommandBarElement> SecondaryCommandsSupplements
        {
            get => (ObservableCollection<ICommandBarElement>) GetValue(SecondaryCommandsSupplementsProperty);
            set => SetValue(SecondaryCommandsSupplementsProperty, value);
        }

        public bool IsDefaultCommandsEnabled
        {
            get => (bool) GetValue(IsDefaultCommandsEnabledProperty);
            set => SetValue(IsDefaultCommandsEnabledProperty, value);
        }

        public object CommandBarContent
        {
            get => GetValue(CommandBarContentProperty);
            set => SetValue(CommandBarContentProperty, value);
        }

        public IllustrationGridViewModel ViewModel
        {
            get => (IllustrationGridViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
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

        private void SelectAllToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            ViewModel.Illustrations.ForEach(v => v.IsSelected = true);
            ViewModel.Illustrations.CollectionChanged += OnIllustrationsCollectionChanged;
        }

        private void SelectAllToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.Illustrations.ForEach(v => v.IsSelected = false);
            ViewModel.Illustrations.CollectionChanged -= OnIllustrationsCollectionChanged;
        }

        private static void OnIllustrationsCollectionChanged(object? _, NotifyCollectionChangedEventArgs args)
        {
            switch (args)
            {
                case {Action: NotifyCollectionChangedAction.Add}:
                    if (args.NewItems is { } newItems)
                    {
                        foreach (IllustrationViewModel? item in newItems)
                        {
                            if (item is not null)
                            {
                                item.IsSelected = true;
                            }
                        }
                    }

                    break;
            }
        }

        private async void AddAllToBookmarkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
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
                UIHelper.ShowTextToastNotification(
                    IllustrationGridCommandBarResources.AddAllToBookmarkToastTitle,
                    IllustrationGridCommandBarResources.AddAllToBookmarkToastContentFormatted.Format(c),
                    AppContext.AppLogoNoCaptionUri);
            }
        }

        private void SaveAllButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void OpenAllInBrowserButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.SelectedIllustrations is {Count: > 15} selected)
            {
                if (await MessageDialogBuilder.CreateOkCancel(
                        this,
                        IllustrationGridCommandBarResources.SelectedTooManyItemsTitle,
                        IllustrationGridCommandBarResources.SelectedTooManyItemsForOpenInBrowserContent)
                    .ShowAsync() == ContentDialogResult.Primary)
                {
                    foreach (var illustrationViewModel in selected)
                    {
                        await Launcher.LaunchUriAsync(MakoHelper.GetIllustrationWebUri(illustrationViewModel.Id));
                    }
                }
            }
        }

        private void ShareButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
