using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.WinUI.Helpers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Extensions;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using WinUI3Utilities.Attributes;
using Microsoft.UI.Xaml.Documents;

namespace Pixeval.Controls;

[DependencyProperty<TextTransformerType>("TransformerType",defaultValueType:DependencyPropertyDefaultValue.New)]
public sealed partial class TranslatableTextBlock : UserControl
{
    public TranslatableTextBlock()
    {
        this.InitializeComponent();
        var listener = new WeakEventListener<HyperlinkButton, object, RoutedEventArgs>(TranslateButton)
        {
            OnEventAction = (instance, source, eventArgs) => { GetTranslation(); },
            OnDetachAction = (weakEventListener) => TranslateButton.Click -= weakEventListener.OnEvent
        };
        TranslateButton.Click += listener.OnEvent;
    }

    private async void GetTranslation()
    {
        var extensionService = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
        if (!extensionService.ActiveImageTransformerCommands.Any())
            return;
        var translator = extensionService.ActiveExtensions.FirstOrDefault(p => p is ITextTransformerCommandExtension) as ITextTransformerCommandExtension;
        if (translator is null) return;
        var text = "";
        foreach (var block in RawText.Blocks)
        {
            if(block is Paragraph p)
            {
                foreach(var i in p.Inlines)
                {
                    if (i is Run r)
                    {
                        text += r.Text;
                    }
                }
            }
        }
        var result = await translator.TransformAsync(text, TransformerType);
        TranslatedText.Text = result;
    }
        
}
