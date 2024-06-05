using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls.Windowing;
using Pixeval.Download;
using Pixeval.Download.MacroParser;
using Pixeval.Download.MacroParser.Ast;
using Pixeval.Download.Macros;
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

    private void DownloadMacroSettingsExpander_OnLoaded(object sender, RoutedEventArgs e)
    {
        Entry.PropertyChanged += (_, _) => EntryOnPropertyChanged();
        EntryOnPropertyChanged();

        return;
        void EntryOnPropertyChanged()
        {
            DownloadPathMacroTextBox.Document.GetText(TextGetOptions.None, out var text);
            var t = text.ReplaceLineEndings("");
            if (t == Entry.Value)
                return;
            // The first time viewmodel get the value of DownloadPathMacro from AppSettings won't trigger the property changed event
            _previousPath = Entry.Value;
            SetPathMacroRichEditBoxDocument(Entry.Value);
        }
    }

    private void DownloadPathMacroTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        DownloadMacroInvalidInfoBar.IsOpen = false;
        _previousPath = Entry.Value;
        if (sender.To<RichEditBox>().Document.Selection is { Length: 0 } selection)
            selection.CharacterFormat.ForegroundColor = Application.Current.GetResource<SolidColorBrush>("TextFillColorPrimaryBrush").Color;
    }

    private void DownloadPathMacroTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (Entry.Value.IsNullOrBlank())
        {
            DownloadMacroInvalidInfoBar.Message = SettingsPageResources.DownloadMacroInvalidInfoBarInputCannotBeBlank;
            DownloadMacroInvalidInfoBar.IsOpen = true;
            Entry.Value = _previousPath;
            return;
        }

        try
        {
            _testParser.SetupParsingEnvironment(new Lexer(Entry.Value));
            var result = _testParser.Parse();
            if (result is not null)
            {
                var legitimatedNames = App.AppViewModel.AppServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
                if (ValidateMacro(result, legitimatedNames) is { } ex)
                {
                    DownloadMacroInvalidInfoBar.Message = ex;
                    DownloadMacroInvalidInfoBar.IsOpen = true;
                    Entry.Value = _previousPath;
                }
                SetPathMacroRichEditBoxDocument(Entry.Value);
            }
        }
        catch (Exception exception)
        {
            DownloadMacroInvalidInfoBar.Message = SettingsPageResources.DownloadMacroInvalidInfoBarMacroInvalidFormatted.Format(exception.Message);
            DownloadMacroInvalidInfoBar.IsOpen = true;
            Entry.Value = _previousPath;
        }
    }

    private void DownloadPathMacroTextBox_OnTextChanged(object sender, RoutedEventArgs e)
    {
        sender.To<RichEditBox>().Document.GetText(TextGetOptions.None, out var text);
        if (sender.To<RichEditBox>().Document.Selection is { Length: 0 } selection)
            selection.CharacterFormat.ForegroundColor = Application.Current.GetResource<SolidColorBrush>("TextFillColorPrimaryBrush").Color;
        Entry.Value = text.ReplaceLineEndings("");
    }

    private void DownloadPathMacroTextBox_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        e.Handled = true;
    }

    private void PathMacroTokenInputBox_OnTokenClick(object sender, ItemClickEventArgs e)
    {
        UiHelper.ClipboardSetText(e.ClickedItem.To<StringRepresentableItem>().StringRepresentation);
        WindowFactory.GetWindowForElement(this).HWnd.SuccessGrowl(SettingsPageResources.MacroCopiedToClipboard);
    }

    private void SetPathMacroRichEditBoxDocument(string path)
    {
        DownloadPathMacroTextBox.Document.BeginUndoGroup();
        DownloadPathMacroTextBox.Document.SetText(TextSetOptions.None, "");
        _pathParser.SetupParsingEnvironment(new Lexer(path));
        if (_pathParser.Parse() is { } result)
        {
            var manipulators = RenderPathRichText(result, DownloadPathMacroTextBox.Document);
            foreach (var ((start, endExclusive), action) in manipulators)
            {
                var textRange = DownloadPathMacroTextBox.Document.GetRange(start, endExclusive);
                action(textRange);
            }
        }
        DownloadPathMacroTextBox.Document.EndUndoGroup();
    }

    private static string? ValidateMacro(
        IMetaPathNode<string> tree, IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask> macroProvider)
    {
        return ValidateMacro(tree, ImmutableDictionary<string, bool>.Empty, [], macroProvider);
    }

    private static string? ValidateMacro(
        IMetaPathNode<string> tree,
        ImmutableDictionary<string, bool> context,
        List<(string Name, ImmutableDictionary<string, bool> Context)> lastSegmentContexts,
        IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask> macroProvider)
    {
        return tree switch
        {
            PlainText<string>(var t) when t.Contains('\\') => lastSegmentContexts.Let(x =>
            {
                foreach (var (name, ctx) in x)
                    if (!ctx.Any(y => context.TryGetValue(y.Key, out var actual) && y.Value != actual))
                        return MacroParserResources.MacroShouldBeInLastSegmentFormatted.Format(name);
                return null;
            }),
            PlainText<string> => null,
            OptionalMacroParameter<string>(var sequence) => ValidateMacro(sequence, context, lastSegmentContexts, macroProvider),
            Macro<string>({ Text: var name }, var optionalParams, var isNot) =>
                macroProvider.PathParser.MacroProvider.TryResolve(name, isNot) switch
                {
                    Unknown => MacroParserResources.UnknownMacroNameFormatted.Format(name),
                    // ITransducer
                    ITransducer when isNot => MacroParserResources.NegationNotAllowedFormatted.Format(name),
                    ITransducer when optionalParams is not null => MacroParserResources.NonParameterizedMacroBearingParameterFormatted.Format(name),
                    MangaIndexMacro m when !(context.TryGetValue(IsMangaMacro.NameConst, out var v) && v) => MacroParserResources.MacroShouldBeContainedFormatted.Format(m.Name, IsMangaMacro.NameConst),
                    ILastSegment l => (null as string).LetChain(_ => lastSegmentContexts.Add((l.Name, context))),
                    ITransducer => null,
                    // IPredicate
                    IPredicate when optionalParams is null => MacroParserResources.ParameterizedMacroMissingParameterFormatted.Format(name),
                    IPredicate p => ValidateMacro(optionalParams, context.Let(t => t.SetItem(p.Name, !p.IsNot)), lastSegmentContexts, macroProvider),
                    _ => MacroParserResources.UnknownMacroNameFormatted.Format(name)
                },
            Sequence<string>(var first, var rests) =>
                ValidateMacro(first, context, lastSegmentContexts, macroProvider)
                ?? (rests is null
                    ? null
                    : ValidateMacro(rests, context, lastSegmentContexts, macroProvider)),
            _ => ThrowHelper.ArgumentOutOfRange<IMetaPathNode<string>, string?>(tree)
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
        Color[] highlightColor = [
            Color.FromArgb(255, 192, 134, 192),
            Color.FromArgb(255, 154, 198, 206),
            Color.FromArgb(255, 220, 220, 163),
            Color.FromArgb(255, 69, 161 ,94)
        ];
        switch (tree)
        {
            case OptionalMacroParameter<string>(var sequence):
                manipulators.AddRange(RenderPathRichText(sequence, document, nestedLevel));
                break;
            case Macro<string>((var name) _, var optionalParameters, var isNot):
                manipulators[AppendDocumentText(document, "@{")] = range => range.CharacterFormat.ForegroundColor = highlightColor[nestedLevel % 4];
                if (isNot)
                {
                    manipulators[AppendDocumentText(document, "!")] = range => range.CharacterFormat.ForegroundColor = highlightColor[nestedLevel % 4];
                }
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
}
