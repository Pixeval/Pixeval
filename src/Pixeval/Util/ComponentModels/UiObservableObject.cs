// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Windowing;

namespace Pixeval.Util.ComponentModels;

public partial class UiObservableObject(FrameworkElement frameworkElement) : ObservableObject
{
    public EnhancedWindow Window => WindowFactory.GetWindowForElement(FrameworkElement);

    public FrameworkElement FrameworkElement { get; } = frameworkElement;
}
