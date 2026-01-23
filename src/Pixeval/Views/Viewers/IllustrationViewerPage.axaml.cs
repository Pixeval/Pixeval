using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace Pixeval.Views.Viewers;

public partial class IllustrationViewerPage : UserControl
{
    public IllustrationViewerPage()
    {
        InitializeComponent();

        AddHandler(Frame.NavigatedToEvent, (sender, e) => DataContext = e.Parameter);
    }
}
