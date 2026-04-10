using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pixeval.Views.Entry;

public partial class UserContainer : UserControl
{
    public event EventHandler<RoutedEventArgs>? RefreshRequested;

    /// <summary>
    /// The command elements that will appear at the left of the TopCommandBar
    /// </summary>
    public ObservableCollection<Control> CommandBarElements { get; } = [];

    public UserContainer()
    {
        InitializeComponent();

        CommandBarElements.CollectionChanged += (_, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: { } newItems })
                foreach (Control argsNewItem in newItems)
                    ExtraCommandsBar.Children.Insert(0, argsNewItem);
            else
                throw new ArgumentException("This collection does not support operations except the Add");
        };
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e) => RefreshRequested?.Invoke(sender, e);
}
