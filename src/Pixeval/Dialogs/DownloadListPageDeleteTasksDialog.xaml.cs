using Microsoft.UI.Xaml;

namespace Pixeval.Dialogs
{
    public sealed partial class DownloadListPageDeleteTasksDialog
    {
        public DownloadListPageDeleteTasksDialog()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DeleteLocalFilesProperty = DependencyProperty.Register(
            nameof(DeleteLocalFiles),
            typeof(bool),
            typeof(DownloadListPageDeleteTasksDialog),
            PropertyMetadata.Create(false));

        public bool DeleteLocalFiles
        {
            get => (bool) GetValue(DeleteLocalFilesProperty);
            set => SetValue(DeleteLocalFilesProperty, value);
        }
    }
}
