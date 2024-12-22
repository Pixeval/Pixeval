using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Pixeval.Controls.Windowing;

namespace Pixeval.Util.ComponentModels;

public partial class UiObservableObject(ulong hWnd) : ObservableObject
{
    public ulong HWnd { get; } = hWnd;

    public FrameworkElement FrameworkElement => WindowFactory.GetContentFromHWnd(HWnd);
}
