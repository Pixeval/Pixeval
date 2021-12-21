using Microsoft.UI.Xaml;
using Pixeval.Misc;

namespace Pixeval.Dialogs;

[DependencyProperty("DeleteLocalFiles", typeof(bool), DefaultValue = "false")]
public sealed partial class DownloadListPageDeleteTasksDialog
{
    public DownloadListPageDeleteTasksDialog()
    {
        InitializeComponent();
    }
    
}