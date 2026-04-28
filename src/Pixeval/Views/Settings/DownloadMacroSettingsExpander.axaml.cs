using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoSettingsPage.Avalonia;
using Avalonia.Interactivity;
using CommunityToolkit.Avalonia.Controls;
using Mako.Model;
using Misaki;
using Pixeval.Download;
using Pixeval.Download.MacroParser;
using Pixeval.Download.MacroParser.Ast;
using Pixeval.Download.Macros;
using Pixeval.I18N;
using Pixeval.Models.Settings.Entries;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Views.Settings;

public partial class DownloadMacroSettingsExpander : SettingsExpander, IEntryControl<DownloadMacroAppSettingsEntry>
{
    public DownloadMacroAppSettingsEntry Entry { set => DataContext = value; }

    public DownloadMacroSettingsExpander() => InitializeComponent();

    /// <summary>
    /// This TestParser is used to test whether the user input meta path is legal
    /// </summary>
    private static readonly MacroParser<string> _TestParser = new();

    private static readonly ISingleImage _SingleImage = DesignHelper.DownloadParserSampleWork(ImageType.SingleImage);
    private static readonly ISingleImage _SingleAnimatedImage = DesignHelper.DownloadParserSampleWork(ImageType.SingleAnimatedImage);
    private static readonly ISingleImage _ImageSet = DesignHelper.DownloadParserSampleWork(ImageType.ImageSet);
    private static readonly ISingleImage _Novel = DesignHelper.DownloadParserSampleWork(ImageType.Other);

    private void DownloadPathMacroTextBox_OnTextChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DownloadMacroAppSettingsEntry entry)
            return;

        var text = DownloadPathMacroTextBox.Text?.ReplaceLineEndings("")?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            DownloadMacroInvalidInfoBar.Text = SettingsMainViewResources.DownloadMacroInvalidInfoBarInputCannotBeBlank;
            DownloadMacroInvalidInfoBar.IsVisible = true;
            return;
        }

        try
        {
            _TestParser.SetupParsingEnvironment(new Lexer(text));
            var result = _TestParser.Parse();
            if (result is not null)
            {
                if (ValidateMacro(result, ArtworkMetaPathParser.Instance) is { } ex)
                {
                    DownloadMacroInvalidInfoBar.Text = ex;
                    DownloadMacroInvalidInfoBar.IsVisible = true;
                }
                else
                {
                    DownloadMacroInvalidInfoBar.IsVisible = false;
                    SetExamplePaths(text);
                    entry.Value = text;
                }
            }
        }
        catch (Exception exception)
        {
            DownloadMacroInvalidInfoBar.Text = I18NManager.GetResource(SettingsMainViewResources.DownloadMacroInvalidInfoBarMacroInvalidFormatted, exception.Message);
            DownloadMacroInvalidInfoBar.IsVisible = true;
        }
    }

    private void SetExamplePaths(string text)
    {
        SinglePathBlock.Text = GetPath(_SingleImage);
        AnimatedPathBlock.Text = GetPath(_SingleAnimatedImage);
        SetPathBlock.Text = IoHelper.ReplaceTokenSetIndex(GetPath(_ImageSet), _ImageSet.SetIndex);
        NovelPathBlock.Text = GetPath(_Novel);
        return;

        string GetPath(IArtworkInfo info) =>
            IoHelper.NormalizePath(ArtworkMetaPathParser.Instance.Reduce(text, info))
                .Replace(FileExtensionMacro.NameConstToken, ".png");
    }

    private static string? ValidateMacro(
        IMetaPathNode<string> tree, IMetaPathParser<Illustration> macroProvider)
    {
        return ValidateMacro(tree, ImmutableDictionary<string, bool>.Empty, [], macroProvider);
    }

    private static string? ValidateMacro(
        IMetaPathNode<string> tree,
        ImmutableDictionary<string, bool> context,
        List<(string Name, ImmutableDictionary<string, bool> Context)> lastSegmentContexts,
        IMetaPathParser<Illustration> macroProvider)
    {
        return tree switch
        {
            PlainText<string>(var t) when t.Contains('\\') => lastSegmentContexts.Let(x =>
            {
                foreach (var (name, ctx) in x)
                    if (!ctx.Any(y => context.TryGetValue(y.Key, out var actual) && y.Value != actual))
                        return I18NManager.GetResource(MacroParserResources.MacroShouldBeInLastSegmentFormatted, name);
                return null;
            }),
            PlainText<string> => null,
            OptionalMacroParameter<string>(var sequence) => ValidateMacro(sequence, context, lastSegmentContexts, macroProvider),
            Macro<string>({ Text: var name }, var optionalParams, var isNot) =>
                macroProvider.MacroProvider.TryResolve(name, isNot) switch
                {
                    Unknown => I18NManager.GetResource(MacroParserResources.UnknownMacroNameFormatted, name),
                    // ITransducer
                    ITransducer when isNot => I18NManager.GetResource(MacroParserResources.NegationNotAllowedFormatted, name),
                    ITransducer when optionalParams is not null => I18NManager.GetResource(MacroParserResources.NonParameterizedMacroBearingParameterFormatted, name),
                    PicSetIndexMacro m when !(context.TryGetValue(IsPicSetMacro.NameConst, out var v) && v) => I18NManager.GetResource(MacroParserResources.MacroShouldBeContainedFormatted, m.Name, IsPicSetMacro.NameConst),
                    ILastSegment l => (null as string).Apply(_ => lastSegmentContexts.Add((l.Name, context))),
                    ITransducer => null,
                    // IPredicate
                    IPredicate when optionalParams is null => I18NManager.GetResource(MacroParserResources.ParameterizedMacroMissingParameterFormatted, name),
                    IPredicate p => ValidateMacro(optionalParams, context.Let(t => t.SetItem(p.Name, !p.IsNot)), lastSegmentContexts, macroProvider),
                    _ => I18NManager.GetResource(MacroParserResources.UnknownMacroNameFormatted, name)
                },
            Sequence<string>(var first, var rests) =>
                ValidateMacro(first, context, lastSegmentContexts, macroProvider)
                ?? (rests is null
                    ? null
                    : ValidateMacro(rests, context, lastSegmentContexts, macroProvider)),
            _ => throw new ArgumentOutOfRangeException(nameof(tree))
        };
    }
}
