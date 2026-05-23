// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Work;

namespace Pixeval.Views.Viewers;

[PseudoClasses(":docked", ":locked")]
public partial class IllustrationViewerInfoPane : UserControl
{
    private IllustrationViewerPageViewModel ViewModel => (IllustrationViewerPageViewModel) DataContext!;

    public static readonly StyledProperty<bool> IsDockedProperty = AvaloniaProperty.Register<IllustrationViewerInfoPane, bool>(
        nameof(IsDocked));

    public static readonly StyledProperty<bool> IsLockedProperty = AvaloniaProperty.Register<IllustrationViewerInfoPane, bool>(
        nameof(IsLocked));

    public bool IsDocked
    {
        get => GetValue(IsDockedProperty);
        set => SetValue(IsDockedProperty, value);
    }

    public bool IsLocked
    {
        get => GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }
    
    public IllustrationViewerInfoPane()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if(change.Property == IsDockedProperty)
            PseudoClasses.Set(":docked", IsDocked);
        if(change.Property == IsLockedProperty)
            PseudoClasses.Set(":locked", IsLocked);
    }

    private async void AddToBookmarkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await BookmarkTagSelectorFlyoutHelper.ShowAsync(
            AddToBookmarkButton,
            SimpleWorkType.IllustrationAndManga,
            AddToBookmarkAsync,
            PlacementMode.TopEdgeAlignedRight);
    }

    private async Task AddToBookmarkAsync((bool isPrivate, IReadOnlyList<string> tags) e)
    {
        if (ViewModel.CurrentIllustration is IWorkViewModel current)
        {
            await current.AddToBookmarkCommand.ExecuteAsync((e.tags, e.isPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(EntryViewerPageResources.AddedToBookmark));
        }
    }
    
    private void ChevronButtonClicked(object? sender, RoutedEventArgs e)
    {
        IsDocked = !IsDocked;
    }
}
