using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.Windowing;
using Pixeval.Download;
using Pixeval.Download.MacroParser;
using Pixeval.Download.MacroParser.Ast;
using Pixeval.Download.Models;
using Pixeval.Settings.Models;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Controls.Settings;

public sealed partial class DownloadMacroSettingsExpander
{
    public DownloadMacroAppSettingsEntry Entry { get; set; } = null!;

    public DownloadMacroSettingsExpander() => InitializeComponent();

    /// <summary>
    /// This TestParser is used to test whether the user input meta path is legal
    /// </summary>
    private static readonly MacroParser<string> _testParser = new();
    private static readonly MacroParser<string> _pathParser = new();

    /// <summary>
    /// The previous meta path after user changes the path field, if the path is illegal
    /// its value will be reverted to this field.
    /// </summary>
    private string _previousPath = "";

    private void DownloadMacroSettingsExpander_OnLoading(FrameworkElement sender, object args)
    {
        Entry.PropertyChanged += (_, _) => EntryOnPropertyChanged();
        EntryOnPropertyChanged();

        return;
        void EntryOnPropertyChanged()
        {
            // The first time viewmodel get the value of DefaultDownloadPathMacro from AppSettings won't trigger the property changed event
            _previousPath = Entry.DefaultDownloadPathMacro;
            SetPathMacroRichEditBoxDocument(Entry.DefaultDownloadPathMacro);
        }
    }

    private void DefaultDownloadPathMacroTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        DownloadMacroInvalidInfoBar.IsOpen = false;
        _previousPath = Entry.DefaultDownloadPathMacro;
        if (sender.To<RichEditBox>().Document.Selection is { Length: 0 } selection)
            selection.CharacterFormat.ForegroundColor = Application.Current.GetResource<SolidColorBrush>("TextFillColorPrimaryBrush").Color;
    }

    private void DefaultDownloadPathMacroTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (Entry.DefaultDownloadPathMacro.IsNullOrBlank())
        {
            DownloadMacroInvalidInfoBar.Message = SettingsPageResources.DownloadMacroInvalidInfoBarInputCannotBeBlank;
            DownloadMacroInvalidInfoBar.IsOpen = true;
            Entry.DefaultDownloadPathMacro = _previousPath;
            return;
        }

        try
        {
            _testParser.SetupParsingEnvironment(new Lexer(Entry.DefaultDownloadPathMacro));
            var result = _testParser.Parse();
            if (result is not null)
            {
                using var scope = App.AppViewModel.AppServicesScope;
                var legitimatedNames = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
                if (ValidateMacro(result, legitimatedNames) is (false, var name))
                {
                    _ = ThrowUtils.MacroParse<MacroParseException>(MacroParserResources.UnknownMacroNameFormatted.Format(name));
                }
                SetPathMacroRichEditBoxDocument(Entry.DefaultDownloadPathMacro);
            }
        }
        catch (Exception exception)
        {
            DownloadMacroInvalidInfoBar.Message = SettingsPageResources.DownloadMacroInvalidInfoBarMacroInvalidFormatted.Format(exception.Message);
            DownloadMacroInvalidInfoBar.IsOpen = true;
            Entry.DefaultDownloadPathMacro = _previousPath;
        }
    }

    private void SetPathMacroRichEditBoxDocument(string path)
    {
        DefaultDownloadPathMacroTextBox.Document.BeginUndoGroup();
        DefaultDownloadPathMacroTextBox.Document.SetText(TextSetOptions.None, "");
        _pathParser.SetupParsingEnvironment(new Lexer(path));
        if (_pathParser.Parse() is { } result)
        {
            var manipulators = RenderPathRichText(result, DefaultDownloadPathMacroTextBox.Document);
            foreach (var ((start, endExclusive), action) in manipulators)
            {
                var textRange = DefaultDownloadPathMacroTextBox.Document.GetRange(start, endExclusive);
                action(textRange);
            }
        }
        DefaultDownloadPathMacroTextBox.Document.EndUndoGroup();
    }

    private static (bool, string?) ValidateMacro(IMetaPathNode<string> tree, IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask> macroProvider)
    {
        return tree switch
        {
            OptionalMacroParameter<string>(var sequence) => sequence is null ? (true, null) : ValidateMacro(sequence, macroProvider),
            Macro<string>(var name, var optionalParams) =>
                Functions.Block(() =>
                    macroProvider.PathParser.MacroProvider.TryResolve(name.Text) is Unknown
                        ? (false, name.Text)
                        : optionalParams is null ? (true, null) : ValidateMacro(optionalParams, macroProvider)),
            PlainText<string> plainText => (true, null),
            Sequence<string>(var first, var rests) =>
                Functions.Block(() =>
                    ValidateMacro(first, macroProvider) is (false, _) result ? result : rests is null ? (true, null) : ValidateMacro(rests, macroProvider)),
            _ => ThrowHelper.ArgumentOutOfRange<IMetaPathNode<string>, (bool, string?)>(tree)
        };
    }

    [GeneratedRegex(@"\\par(?!d)")]
    private static partial Regex RtpNewLineRegex();

    private static (int start, int endExclusive) AppendDocumentText(RichEditTextDocument document, string text)
    {
        return InsertDocumentText(document, text, -1);
    }

    private static (int start, int endExclusive) InsertDocumentText(RichEditTextDocument document, string text, int position)
    {
        document.GetText(TextGetOptions.None, out var txt);
        // workaround for stupid issue: https://github.com/microsoft/microsoft-ui-xaml/issues/1941 
        // screw WinUI team for not fixing this for ~3y.
        document.SetText(TextSetOptions.None, position is -1 ? txt + text : txt.Insert(position, text));
        document.GetText(TextGetOptions.FormatRtf, out var rtf);
        document.SetText(TextSetOptions.FormatRtf, RtpNewLineRegex().Replace(rtf, ""));

        return position is -1 ? (txt.Length - 1, txt.Length + text.Length - 1) : (position - 1, position - 1);
    }

    // ReSharper disable TailRecursiveCall
    private static Dictionary<(int start, int endExclusive), Action<ITextRange>> RenderPathRichText(IMetaPathNode<string> tree, RichEditTextDocument document, int nestedLevel = 0)
    {
        var manipulators = new Dictionary<(int start, int endExclusive), Action<ITextRange>>();
        List<Color> highlightColor = [
            Color.FromArgb(255, 192, 134, 192),
            Color.FromArgb(255, 154, 198, 206),
            Color.FromArgb(255, 220, 220, 163),
            Color.FromArgb(255, 69, 161 ,94)
        ];
        switch (tree)
        {
            case OptionalMacroParameter<string>(var sequence):
                if (sequence is not null)
                {
                    manipulators.AddRange(RenderPathRichText(sequence, document, nestedLevel));
                }
                break;
            case Macro<string>((var name) _, var optionalParameters):
                manipulators[AppendDocumentText(document, "@{")] = range => range.CharacterFormat.ForegroundColor = highlightColor[nestedLevel % 4];
                manipulators[AppendDocumentText(document, name)] = range => range.CharacterFormat.ForegroundColor = highlightColor[nestedLevel % 4];
                if (optionalParameters is not null)
                {
                    manipulators[AppendDocumentText(document, "=")] = range => range.CharacterFormat.ForegroundColor = highlightColor[nestedLevel % 4];
                    manipulators.AddRange(RenderPathRichText(optionalParameters, document, nestedLevel + 1));
                }

                manipulators[AppendDocumentText(document, "}")] = range => range.CharacterFormat.ForegroundColor = highlightColor[nestedLevel % 4];
                break;
            case Sequence<string>(var first, var remains):
                manipulators.AddRange(RenderPathRichText(first, document, nestedLevel));
                if (remains is not null)
                {
                    manipulators.AddRange(RenderPathRichText(remains, document, nestedLevel));
                }
                break;
            case PlainText<string>(var text):
                _ = AppendDocumentText(document, text);
                break;
            default:
                return manipulators;
        }

        return manipulators;
    }

    private void DefaultDownloadPathMacroTextBox_OnTextChanged(object sender, RoutedEventArgs e)
    {
        sender.To<RichEditBox>().Document.GetText(TextGetOptions.None, out var text);
        if (sender.To<RichEditBox>().Document.Selection is { Length: 0 } selection)
            selection.CharacterFormat.ForegroundColor = Application.Current.GetResource<SolidColorBrush>("TextFillColorPrimaryBrush").Color;
        Entry.DefaultDownloadPathMacro = text.ReplaceLineEndings("");
    }

    private void DefaultDownloadPathMacroTextBox_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        e.Handled = true;
    }

    private void PathMacroTokenInputBox_OnTokenTapped(object sender, ItemClickEventArgs e)
    {
        UiHelper.ClipboardSetText(e.ClickedItem.To<StringRepresentableItem>().StringRepresentation);
        WindowFactory.GetWindowForElement(this).HWnd.SuccessGrowl(SettingsPageResources.MacroCopiedToClipboard);
    }

    private void DefaultDownloadPathMacroEntry_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = this.CreateAcknowledgementAsync(SettingsPageResources.MacroTutorialDialogTitle, SettingsPageResources.MacroTutorialDialogContent);
    }
}
