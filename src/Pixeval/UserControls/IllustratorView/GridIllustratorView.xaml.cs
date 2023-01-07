using System;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Pixeval.UserControls.IllustratorView;

public sealed partial class GridIllustratorView : IIllustratorView
{
    public GridIllustratorView()
    {
        InitializeComponent();
        ViewModel = new GridIllustratorViewViewModel();
    }

    private EventHandler<TappedRoutedEventArgs>? _userTapped;

    public event EventHandler<TappedRoutedEventArgs> UserTapped
    {
        add => _userTapped += value;
        remove => _userTapped -= value;
    }

    public GridIllustratorViewViewModel ViewModel { get; }

    public FrameworkElement SelfIllustratorView => this;

    IllustratorViewViewModel IIllustratorView.ViewModel => ViewModel;

    public ScrollViewer ScrollViewer => IllustratorGridView.FindDescendant<ScrollViewer>()!;

    public UIElement? GetItemContainer(IllustratorViewModel viewModel)
    {
        return IllustratorGridView.ContainerFromItem(viewModel) as UIElement;
    }

    private void IllustratorProfile_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _userTapped?.Invoke(sender, e);
    }
}