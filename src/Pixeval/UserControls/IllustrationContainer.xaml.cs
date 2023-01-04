#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationContainer.xaml.cs
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Options;
using Pixeval.UserControls.IllustrationView;
using WinUI3Utilities.Attributes;


namespace Pixeval.UserControls;

[DependencyProperty<ObservableCollection<ICommandBarElement>>("PrimaryCommandsSupplements", DefaultValue = "new ObservableCollection<ICommandBarElement>()")]
[DependencyProperty<ObservableCollection<ICommandBarElement>>("SecondaryCommandsSupplements", DefaultValue = "new ObservableCollection<ICommandBarElement>()")]
[DependencyProperty<object>("Header")]
public sealed partial class IllustrationContainer
{
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
        IllustrationView = App.AppViewModel.AppSetting.IllustrationViewOption switch
        {
            IllustrationViewOption.Regular => new GridIllustrationView(),
            IllustrationViewOption.Justified => new JustifiedLayoutIllustrationView(),
            _ => throw new ArgumentOutOfRangeException()
        }; 
        IllustrationContainerDockPanel.Children.Add(IllustrationView.SelfIllustrationView);
    }

    public IllustrationViewViewModel ViewModel => IllustrationView.ViewModel;

    public IIllustrationView IllustrationView { get; set; }


    /// <summary>
    ///     The command elements that will appear at the left of the <see cref="TopCommandBar" />
    /// </summary>
    public ObservableCollection<UIElement> CommandBarElements { get; }

    private void IllustrationContainer_OnLoaded(object sender, RoutedEventArgs e)
    {
        IllustrationView.SelfIllustrationView.Focus(FocusState.Programmatic);
    }

    public void ScrollToTop()
    {
        IllustrationView.ScrollViewer.ChangeView(null, 0, null, false);
    }
}
