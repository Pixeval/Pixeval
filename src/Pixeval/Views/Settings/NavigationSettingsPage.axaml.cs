// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.AppManagement;
using Pixeval.I18N;
using Pixeval.Models.Navigation;
using Pixeval.Utilities;

namespace Pixeval.Views.Settings;

public sealed partial class NavigationSettingsPage : ContentPage
{
    private readonly NavigationYamlEditorColorizer _colorizer = new();
    private bool _isSynchronizingText;

    public ObservableCollection<NavigationDiagnostic> Diagnostics { get; } = [];

    public NavigationSettingsPage()
    {
        InitializeComponent();
        DataContext = this;
        YamlEditor.TextArea.TextView.LineTransformers.Add(_colorizer);
        SetEditorText(App.AppViewModel.NavigationMenuYamlText);
        ValidateCurrentText();
    }

    private void ApplyButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        var result = ValidateCurrentText();
        if (result.Configuration is null)
            return;

        App.AppViewModel.NavigationMenuYamlText = YamlEditor.Text ?? "";
        AppInfo.SaveNavigationMenuYaml(App.AppViewModel.NavigationMenuYamlText);
        TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
            I18NManager.GetResource(NavigationSettingsPageResources.Applied));

        if (TopLevel.GetTopLevel(this)?.ViewContainer is Views.ViewContainers.TabViewContainer container)
            container.ReloadNavigation();
    }

    private void FormatButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        var result = ValidateCurrentText();
        if (result.Configuration is not { } configuration)
            return;

        SetEditorText(NavigationYamlFormatter.Format(configuration));
        ValidateCurrentText();
    }

    private void ResetButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        SetEditorText(NavigationMenuYaml.DefaultYaml);
        ValidateCurrentText();
    }

    private void InsertFolderButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        InsertYaml(
            $"""

               - folder: "${NavigationSettingsPageResources.DefaultFolderTitle}"
                 icon: DocumentFolder
                 children:
                   - page: Home

             """);
    }

    private void YamlEditor_OnTextChanged(object? sender, EventArgs e)
    {
        if (!_isSynchronizingText)
            ValidateCurrentText();
    }

    private NavigationParseResult ValidateCurrentText()
    {
        var result = NavigationYamlParser.Parse(YamlEditor.Text);

        Diagnostics.Clear();
        foreach (var diagnostic in result.Diagnostics)
            Diagnostics.Add(diagnostic);

        _colorizer.Update(result.Diagnostics, YamlEditor.Text?.Length ?? 0);
        YamlEditor.TextArea.TextView.InvalidateVisual();
        CanApplyButton.IsEnabled = result.Configuration is not null;
        StatusTextBlock.Text = GetStatusText(result);
        return result;
    }

    private void SetEditorText(string text)
    {
        if (string.Equals(YamlEditor.Text, text, StringComparison.Ordinal))
            return;

        _isSynchronizingText = true;
        try
        {
            YamlEditor.Text = text;
        }
        finally
        {
            _isSynchronizingText = false;
        }
    }

    private void InsertYaml(string snippet)
    {
        var textArea = YamlEditor.TextArea;
        var caret = textArea.Caret.Offset;
        YamlEditor.Document.Insert(caret, snippet);
        textArea.Caret.Offset = caret + snippet.Length;
        ValidateCurrentText();
    }

    private static string GetStatusText(NavigationParseResult result)
    {
        if (result.Configuration is null)
            return I18NManager.GetResource(NavigationSettingsPageResources.StatusInvalidFormatted, result.Diagnostics.Count);

        return result.Diagnostics.Count is 0
            ? I18NManager.GetResource(NavigationSettingsPageResources.StatusValid)
            : I18NManager.GetResource(NavigationSettingsPageResources.StatusValidWithWarningsFormatted, result.Diagnostics.Count);
    }
}
