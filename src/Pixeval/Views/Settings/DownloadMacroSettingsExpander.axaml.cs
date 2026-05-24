// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using AutoSettingsPage.Avalonia;
using CommunityToolkit.Avalonia.Controls;
using Misaki;
using Pixeval.Controls.Settings;
using Pixeval.Download;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.I18N;
using Pixeval.Models.Settings.Entries;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Views.Settings;

public partial class DownloadMacroSettingsExpander : SettingsExpander, IEntryControl<DownloadMacroAppSettingsEntry>
{
    private readonly MacroEditorColorizer _colorizer = new();
    private DownloadMacroAppSettingsEntry? _entry;
    private bool _isSynchronizingText;

    private static readonly ISingleImage _SingleImage = DesignHelper.DownloadParserSampleWork(ImageType.SingleImage);
    private static readonly ISingleImage _SingleAnimatedImage = DesignHelper.DownloadParserSampleWork(ImageType.SingleAnimatedImage);
    private static readonly ISingleImage _ImageSet = DesignHelper.DownloadParserSampleWork(ImageType.ImageSet);
    private static readonly ISingleImage _Novel = DesignHelper.DownloadParserSampleWork(ImageType.Other);

    public DownloadMacroAppSettingsEntry Entry
    {
        set
        {
            DataContext = value;
            _entry = value;
            SyncTextFromEntry(value.Value);
            ApplyText(value.Value);
        }
    }

    public DownloadMacroSettingsExpander()
    {
        InitializeComponent();
        DownloadPathMacroTextBox.TextArea.TextView.LineTransformers.Add(_colorizer);
        DownloadPathMacroTextBox.TextChanged += DownloadPathMacroTextBox_OnTextChanged;
    }

    private void DownloadPathMacroTextBox_OnTextChanged(object? sender, EventArgs e)
    {
        if (_entry is null || _isSynchronizingText)
            return;

        ApplyText(DownloadPathMacroTextBox.Text?.ReplaceLineEndings("")?.Trim());
    }

    private void ApplyText(string? text)
    {
        var normalized = text ?? "";
        var analysis = ArtworkMetaPathAnalyzer.Analyze(normalized);
        _colorizer.Update(analysis, normalized.Length);
        DownloadPathMacroTextBox.TextArea.TextView.InvalidateVisual();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            DownloadMacroInvalidInfoBar.Text = SettingsMainViewResources.DownloadMacroInvalidInfoBarInputCannotBeBlank;
            DownloadMacroInvalidInfoBar.IsVisible = true;
            return;
        }

        if (analysis.Diagnostics.Count > 0)
        {
            DownloadMacroInvalidInfoBar.Text = FormatDiagnostic(analysis.Diagnostics[0]);
            DownloadMacroInvalidInfoBar.IsVisible = true;
            return;
        }

        try
        {
            SetExamplePaths(normalized);
            DownloadMacroInvalidInfoBar.IsVisible = false;
            _entry?.Value = normalized;
        }
        catch (Exception exception)
        {
            DownloadMacroInvalidInfoBar.Text = I18NManager.GetResource(SettingsMainViewResources.DownloadMacroInvalidInfoBarMacroInvalidFormatted, exception.Message);
            DownloadMacroInvalidInfoBar.IsVisible = true;
        }
    }

    private void SyncTextFromEntry(string text)
    {
        if (string.Equals(DownloadPathMacroTextBox.Text, text, StringComparison.Ordinal))
            return;

        _isSynchronizingText = true;
        try
        {
            DownloadPathMacroTextBox.Text = text;
        }
        finally
        {
            _isSynchronizingText = false;
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

    private static string FormatDiagnostic(MacroDiagnostic diagnostic)
    {
        return diagnostic.Kind switch
        {
            MacroDiagnosticKind.UnexpectedToken => I18NManager.GetResource(
                MacroParserResources.UnexpectedTokenFormatted,
                diagnostic.Span.Start + 1),
            MacroDiagnosticKind.UnknownMacroName => I18NManager.GetResource(
                MacroParserResources.UnknownMacroNameFormatted,
                diagnostic.PrimaryParameter),
            MacroDiagnosticKind.NonParameterizedMacroBearingParameter => I18NManager.GetResource(
                MacroParserResources.NonParameterizedMacroBearingParameterFormatted,
                diagnostic.PrimaryParameter),
            MacroDiagnosticKind.ConditionalBranchesMissing => I18NManager.GetResource(
                MacroParserResources.ParameterizedMacroMissingParameterFormatted,
                diagnostic.PrimaryParameter),
            MacroDiagnosticKind.MacroShouldBeContained => I18NManager.GetResource(
                MacroParserResources.MacroShouldBeContainedFormatted,
                diagnostic.PrimaryParameter,
                diagnostic.SecondaryParameter),
            MacroDiagnosticKind.MacroShouldBeInLastSegment => I18NManager.GetResource(
                MacroParserResources.MacroShouldBeInLastSegmentFormatted,
                diagnostic.PrimaryParameter),
            _ => throw new ArgumentOutOfRangeException(nameof(diagnostic))
        };
    }
}
