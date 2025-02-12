// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Extensions;
using Pixeval.Extensions.Common.Commands.Transformers;
using Microsoft.UI.Xaml.Documents;

namespace Pixeval.Controls;

public sealed partial class TranslatableTextBlock : UserControl
{
    [GeneratedDependencyProperty]
    public partial TextTransformerType TransformerType { get; set; }

    public TranslatableTextBlock() => InitializeComponent();

    private async void GetTranslationClicked(object sender, RoutedEventArgs e)
    {
        var extensionService = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
        if (extensionService.ActiveExtensions.FirstOrDefault(p => p is ITextTransformerCommandExtension) is not ITextTransformerCommandExtension translator)
            return;
        var text = "";
        foreach (var block in RawText.Blocks)
            if (block is Paragraph p)
                foreach (var i in p.Inlines)
                    if (i is Run r)
                        text += r.Text;
        if (text == "")
            return;
        var result = await translator.TransformAsync(text, TransformerType);
        TranslatedText.Text = result;
    }
}
