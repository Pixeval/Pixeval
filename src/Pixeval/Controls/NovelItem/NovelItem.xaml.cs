using System;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Messages;
using Pixeval.Options;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<NovelItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class NovelItem : ConstrainedBox
{
    public event Action<NovelItem, NovelItemViewModel>? ViewModelChanged;

    public NovelItem() => InitializeComponent();

    public event Func<TeachingTip> RequestTeachingTip = null!;

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NovelItem item)
        {
            item.ViewModelChanged?.Invoke(item, item.ViewModel);
        }
    }

    private void TagButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        _ = WeakReferenceMessenger.Default.Send(new WorkTagClickedMessage(SimpleWorkType.Novel, (string)((Button)sender).Content));
    }
}
