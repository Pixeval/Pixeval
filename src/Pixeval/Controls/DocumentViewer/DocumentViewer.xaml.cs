using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;
using Pixeval.Utilities;

namespace Pixeval.Controls;

[DependencyProperty<long>("NovelId", "-1", nameof(OnNovelIdChanged))]
public sealed partial class DocumentViewer : UserControl
{
    private DocumentViewerViewModel? ViewModel { get; set; }

    public DocumentViewer()
    {
        InitializeComponent();
    }

    public static async void OnNovelIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var viewer = d.To<DocumentViewer>();
        if (viewer.NovelId is -1)
        {
            viewer.ViewModel = null;
            return;
        }

        viewer.ViewModel = await DocumentViewerViewModel.CreateAsync(viewer.NovelId);
        viewer.NovelRichTextBlock.Blocks.AddRange(viewer.ViewModel.Paragraphs);
        viewer.ViewModel.PropertyChanged += (s, args) =>
        {
            var vm = s.To<DocumentViewerViewModel>();
            switch (args.PropertyName)
            {
                case nameof(vm.CurrentParagraph):
                    viewer.NovelRichTextBlock.Blocks.Clear();
                    viewer.NovelRichTextBlock.Blocks.Add(vm.CurrentParagraph);
                    break;
            }
        };
    }
}
