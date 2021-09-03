using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    public sealed partial class IllustrationContainer
    {
        public IllustrationGridViewModel ViewModel => IllustrationGrid.ViewModel;

        public IllustrationContainer()
        {
            InitializeComponent();
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
                                TopCommandBar.CommandBarElements.Add(argsNewItem);
                            }
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        public ObservableCollection<UIElement> CommandBarElements { get; }

        private void IllustrationContainer_OnLoaded(object sender, RoutedEventArgs e)
        {
            IllustrationGrid.Focus(FocusState.Programmatic);
        }
    }
}
