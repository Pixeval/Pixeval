using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

/// <summary>
/// Defines an area where you can arrange child elements either horizontally or vertically, relative to each other.
/// </summary>
[DependencyProperty<bool>("LastChildFill", "true", nameof(OnPropertyChanged))]
[DependencyProperty<Thickness>("Padding", DependencyPropertyDefaultValue.Default, nameof(OnPropertyChanged))]
[DependencyProperty<double>("HorizontalSpacing", DependencyPropertyDefaultValue.Default, nameof(OnPropertyChanged))]
[DependencyProperty<double>("VerticalSpacing", DependencyPropertyDefaultValue.Default, nameof(OnPropertyChanged))]
public partial class DockPanel
{
    /// <summary>
    /// Gets or sets a value that indicates the position of a child element within a parent <see cref="DockPanel"/>.
    /// </summary>
    public static readonly DependencyProperty DockProperty = DependencyProperty.RegisterAttached(
        nameof(Dock),
        typeof(Dock),
        typeof(FrameworkElement),
        new PropertyMetadata(Dock.Left, DockChanged));

    /// <summary>
    /// Gets DockProperty attached property
    /// </summary>
    /// <param name="obj">Target FrameworkElement</param>
    /// <returns>Dock value</returns>
    public static Dock GetDock(FrameworkElement obj)
    {
        return (Dock)obj.GetValue(DockProperty);
    }

    /// <summary>
    /// Sets DockProperty attached property
    /// </summary>
    /// <param name="obj">Target FrameworkElement</param>
    /// <param name="value">Dock Value</param>
    public static void SetDock(FrameworkElement obj, Dock value)
    {
        obj.SetValue(DockProperty, value);
    }
}
