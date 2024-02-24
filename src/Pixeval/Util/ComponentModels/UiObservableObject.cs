using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Windowing;

namespace Pixeval.Util.ComponentModels;

public class UiObservableObject : ObservableObject
{
    public Window Window { get; private init; }

    private readonly FrameworkElement _frameworkElement;

    public FrameworkElement FrameworkElement
    {
        get => _frameworkElement;
        [MemberNotNull(nameof(_frameworkElement), nameof(Window))]
        private init
        {
            _frameworkElement = value;
            Window = WindowFactory.GetWindowForElement(_frameworkElement);
        }
    }

    public UiObservableObject(FrameworkElement element) => FrameworkElement = element;
}
