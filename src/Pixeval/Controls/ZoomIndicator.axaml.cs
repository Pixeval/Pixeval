using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Pixeval.Controls;

public partial class ZoomIndicator : UserControl
{
    public static readonly FuncValueConverter<int, string> PlusOneConverter = new(i => (i + 1).ToString());
    
    public ZoomIndicator()
    {
        InitializeComponent();
    }
}
