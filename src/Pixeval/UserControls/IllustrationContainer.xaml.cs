using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
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

        /// <summary>
        /// The command elements that will appear at the left of the <see cref="TopCommandBar"/>
        /// </summary>
        public ObservableCollection<UIElement> CommandBarElements { get; }

        private void IllustrationContainer_OnLoaded(object sender, RoutedEventArgs e)
        {
            IllustrationGrid.Focus(FocusState.Programmatic);
        }

        public void ScrollToTop()
        {
            IllustrationGrid.IllustrationGridView.FindDescendant<ScrollViewer>()?.ChangeView(null, 0, null, false);
        }
    }
}
