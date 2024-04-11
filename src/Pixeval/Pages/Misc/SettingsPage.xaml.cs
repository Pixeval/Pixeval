#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SettingsPage.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using Windows.System;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Database.Managers;
using Pixeval.Download.MacroParser;
using Pixeval.Download.MacroParser.Ast;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.Options;
using WinUI3Utilities;
using WinRT;
using System.Text.RegularExpressions;
using Windows.UI;
using Pixeval.Download.Models;
using Pixeval.Download;

namespace Pixeval.Pages.Misc;

[INotifyPropertyChanged]
public sealed partial class SettingsPage : IDisposable
{
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

    private SettingsPageViewModel ViewModel { get; set; } = null!;

    private bool _disposed;

    public SettingsPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        ViewModel = new SettingsPageViewModel(Window.Content.To<FrameworkElement>());
        // The first time viewmodel get the value of DefaultDownloadPathMacro from AppSettings won't trigger the property changed event
        SetPathMacroRichEditBoxDocument(ViewModel.DefaultDownloadPathMacro);
        _previousPath = ViewModel.DefaultDownloadPathMacro;
    }

    public override void OnPageDeactivated(NavigatingCancelEventArgs e) => Dispose();

    private void SettingsPage_OnUnloaded(object sender, RoutedEventArgs e) => Dispose();

    private void Theme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        WindowFactory.SetTheme(ViewModel.Theme);
    }

    private void Backdrop_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        WindowFactory.SetBackdrop(ViewModel.Backdrop);
    }

    private void ImageMirrorServerTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        ImageMirrorServerTextBoxTeachingTip.IsOpen = false;
    }

    private void ImageMirrorServerTextBox_OnLosingFocus(UIElement sender, LosingFocusEventArgs args)
    {
        if (ViewModel.MirrorHost.IsNotNullOrEmpty() && Uri.CheckHostName(ViewModel.MirrorHost) is UriHostNameType.Unknown)
        {
            ImageMirrorServerTextBox.Text = "";
            ImageMirrorServerTextBoxTeachingTip.IsOpen = true;
        }
    }

    private void CheckForUpdateButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.CheckForUpdate();
    }

    private async void OpenHyperlinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await Launcher.LaunchUriAsync(new Uri(sender.To<FrameworkElement>().GetTag<string>()));
    }

    private async void ReleaseNotesHyperlink_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await this.CreateAcknowledgementAsync(SettingsPageResources.ReleaseNotesHyperlinkButtonContent,
            new ScrollView
            {
                Content = new MarkdownTextBlock
                {
                    Config = new MarkdownConfig(),
                    Text = (sender.To<FrameworkElement>().GetTag<string>() is "Newest"
                        ? AppInfo.AppVersion.NewestAppReleaseModel
                        : AppInfo.AppVersion.CurrentAppReleaseModel)?.ReleaseNote ?? ""
                }
            });
    }

    private async void PerformSignOutButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.SignOutConfirmationDialogTitle,
                SettingsPageResources.SignOutConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            App.AppViewModel.LoginContext.LogoutExit = true;
            // Close 不触发 Closing 事件
            AppInfo.SaveContextWhenExit();
            Window.Close();
        }
    }

    private async void ResetDefaultSettingsButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await this.CreateOkCancelAsync(SettingsPageResources.ResetSettingConfirmationDialogTitle,
                SettingsPageResources.ResetSettingConfirmationDialogContent) is ContentDialogResult.Primary)
        {
            ViewModel.ResetDefault();
            OnPropertyChanged(nameof(ViewModel));
        }
    }
    private void DefaultDownloadPathMacroTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        DownloadMacroInvalidTeachingTip.IsOpen = false;
        _previousPath = ViewModel.DefaultDownloadPathMacro;
        sender.As<RichEditBox>().Document.Selection.CharacterFormat.ForegroundColor = Colors.White;
    }

    private void DefaultDownloadPathMacroTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (_disposed)
            return;
        if (ViewModel.DefaultDownloadPathMacro.IsNullOrBlank())
        {
            DownloadMacroInvalidTeachingTip.Subtitle = SettingsPageResources.DownloadMacroInvalidTeachingTipInputCannotBeBlank;
            DownloadMacroInvalidTeachingTip.IsOpen = true;
            ViewModel.DefaultDownloadPathMacro = _previousPath;
            return;
        }

        try
        {
            _testParser.SetupParsingEnvironment(new Lexer(ViewModel.DefaultDownloadPathMacro));
            var result = _testParser.Parse();
            if (result is not null)
            {
                using var scope = App.AppViewModel.AppServicesScope;
                var legitimatedNames = scope.ServiceProvider.GetRequiredService<IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask>>();
                if (ValidateMacro(result, legitimatedNames) is (false, var name))
                {
                    _ = ThrowUtils.MacroParse<MacroParseException>(MacroParserResources.UnknownMacroNameFormatted.Format(name));
                }
                SetPathMacroRichEditBoxDocument(ViewModel.DefaultDownloadPathMacro);
            }
        }
        catch (Exception exception)
        {
            DownloadMacroInvalidTeachingTip.Subtitle = SettingsPageResources.DownloadMacroInvalidTeachingTipMacroInvalidFormatted.Format(exception.Message);
            DownloadMacroInvalidTeachingTip.IsOpen = true;
            ViewModel.DefaultDownloadPathMacro = _previousPath;
        }
    }

    private void SetPathMacroRichEditBoxDocument(string path)
    {
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
    }

    private static (bool, string?) ValidateMacro(IMetaPathNode<string> tree, IDownloadTaskFactory<IllustrationItemViewModel, IllustrationDownloadTask> macroProvider)
    {
        return tree switch
        {
            OptionalMacroParameter<string>(var sequence) => sequence is null ? (true, null) : ValidateMacro(sequence, macroProvider),
            Macro<string>(var name, var optionalParams) =>
                Functions.Block(() =>
                    macroProvider.PathParser.MacroProvider.TryResolve(name.Text) is IMacro<IllustrationItemViewModel>.Unknown
                        ? (false, name.Text)
                        : optionalParams is null ? (true, null) : ValidateMacro(optionalParams, macroProvider)),
            PlainText<string> plainText => (true, null),
            Sequence<string>(var first, var rests) =>
                Functions.Block(() =>
                    ValidateMacro(first, macroProvider) is (false, _) result ? result : rests is null ? (true, null) : ValidateMacro(rests, macroProvider)),
            _ => throw new ArgumentOutOfRangeException(nameof(tree))
        };
    }

    [GeneratedRegex(@"\\par(?!d)")]
    private static partial Regex RtpNewLineRegex();

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

    private static (int start, int endExclusive) AppendDocumentText(RichEditTextDocument document, string text)
    {
        return InsertDocumentText(document, text, -1);
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
        sender.As<RichEditBox>().Document.GetText(TextGetOptions.None, out var text);
        sender.As<RichEditBox>().Document.Selection.CharacterFormat.ForegroundColor = Colors.White;
        ViewModel.DefaultDownloadPathMacro = text.ReplaceLineEndings("");
    }

    private void DefaultDownloadPathMacroTextBox_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        e.Handled = true;
    }

    private void PathMacroTokenInputBox_OnTokenTapped(object sender, ItemClickEventArgs e)
    {
        UiHelper.ClipboardSetText(e.ClickedItem.To<StringRepresentableItem>().StringRepresentation);
        this.ShowTeachingTipAndHide(SettingsPageResources.MacroCopiedToClipboard);
    }

    private void DeleteFileCacheEntryButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = AppKnownFolders.Cache.ClearAsync();
        ViewModel.ShowClearData(ClearDataKind.FileCache);
    }

    private void DeleteSearchHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        manager.Clear();
        ViewModel.ShowClearData(ClearDataKind.SearchHistory);
    }

    private void DeleteBrowseHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<BrowseHistoryPersistentManager>();
        manager.Clear();
        ViewModel.ShowClearData(ClearDataKind.BrowseHistory);
    }

    private void DeleteDownloadHistoriesButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        using var scope = App.AppViewModel.AppServicesScope;
        var manager = scope.ServiceProvider.GetRequiredService<DownloadHistoryPersistentManager>();
        manager.Clear();
        ViewModel.ShowClearData(ClearDataKind.DownloadHistory);
    }

    private void OpenFolder_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var folder = sender.To<FrameworkElement>().GetTag<string>() switch
        {
            "Log" => AppKnownFolders.Log.Self,
            "Temp" => AppKnownFolders.Temporary.Self,
            "Local" => AppKnownFolders.Local.Self,
            "Roaming" => AppKnownFolders.Roaming.Self,
            _ => null
        };
        if (folder is not null)
            _ = Launcher.LaunchFolderAsync(folder);
    }

    private void DefaultDownloadPathMacroEntry_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = this.CreateAcknowledgementAsync(SettingsPageResources.MacroTutorialDialogTitle, SettingsPageResources.MacroTutorialDialogContent);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Bindings.StopTracking();
        ViewModel.SaveCollections();
        App.AppViewModel.AppSettings = ViewModel.AppSetting;
        AppInfo.SaveConfig(ViewModel.AppSetting);
        ViewModel.Dispose();
        ViewModel = null!;
    }
}
