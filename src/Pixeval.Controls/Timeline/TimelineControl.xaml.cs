// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using WinUI3Utilities.Attributes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixeval.Controls.Timeline
{
    [DependencyProperty<DataTemplate>("ItemTemplate")]
    public sealed partial class TimelineControl
    {
        public TimelineControl()
        {
            InitializeComponent();
        }
    }
}
