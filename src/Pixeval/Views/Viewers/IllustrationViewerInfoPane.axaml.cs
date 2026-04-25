using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;

namespace Pixeval.Views.Viewers;

[PseudoClasses(":docked")]
public partial class IllustrationViewerInfoPane : UserControl
{
    public static readonly StyledProperty<bool> IsDockedProperty = AvaloniaProperty.Register<IllustrationViewerInfoPane, bool>(
        nameof(IsDocked));

    public bool IsDocked
    {
        get => GetValue(IsDockedProperty);
        set => SetValue(IsDockedProperty, value);
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
    }

    private async void AddToBookmarkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Complete Migration
        // if (!BookmarkTagSelector.IsVisible)
        //     await BookmarkTagSelector.ResetSourceAsync();
        // BookmarkTagSelector.IsVisible = !BookmarkTagSelector.IsVisible;
    }
    
    private void ChevronButtonClicked(object? sender, RoutedEventArgs e)
    {
        IsDocked = !IsDocked;
        // EntryViewerFloatingPaneView.IsDocked = !EntryViewerFloatingPaneView.IsDocked;
    }
}
