using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Tags;

[DependencyProperty<TagsEntryViewModel>("ViewModel")]
public sealed partial class TagsEntry : IViewModelControl
{
    object IViewModelControl.ViewModel => ViewModel;

    public TagsEntry() => InitializeComponent();

    private void TagButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
    }
}
