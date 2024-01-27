using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Controls;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Pages.Tags;

[DependencyProperty<TagsEntryViewModel>("ViewModel")]
public sealed partial class TagsEntry : IViewModelControl
{
    object IViewModelControl.ViewModel => ViewModel;

    public event Action<TagsEntry, string>? TagTapped;

    public TagsEntry() => InitializeComponent();

    private void TagButton_OnTapped(object sender, TappedRoutedEventArgs e) => TagTapped?.Invoke(this, sender.To<Button>().Content.To<string>());
}
