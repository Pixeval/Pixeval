#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Core;
using Pixeval.Data.ViewModel;
using Pixeval.Objects.Generic;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for PixevalSettingPage.xaml
    /// </summary>
    public partial class SettingsControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }
        
        public static DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(SettingsControl), new FrameworkPropertyMetadata(false, IsOpenPropertyChanged));

        public bool IsOpen
        {
            get => (bool) GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        private static void IsOpenPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingsControl) dependencyObject;
            if (!(bool) e.NewValue)
                control.MacroCompletionPopup.CloseControl();
        }
        
        private void OpenFileDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            using var fileDialog = new CommonOpenFileDialog(AkaI18N.PleaseSelectLocation) { InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), IsFolderPicker = true };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DownloadPathTextBox.Text = fileDialog.FileName;
            }
        }

        private void QueryR18_OnChecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>();
            if (Settings.Global.ExcludeTag != null)
            {
                set.AddRange(Settings.Global.ExcludeTag);
            }
            set.AddRange(new[] { "R-18", "R-18G" });
            Settings.Global.ExcludeTag = set;
        }

        private void QueryR18_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var set = new HashSet<string>(Settings.Global.ExcludeTag);
            set.Remove("R-18");
            set.Remove("R-18G");
            Settings.Global.ExcludeTag = set;
        }

        private void ExcludeTagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = ExcludeTagTextBox.Text.Split(" ");
            Settings.Global.ExcludeTag = new HashSet<string>(text);
        }

        private void IncludeTagTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var text = IncludeTagTextBox.Text.Split(" ");
            Settings.Global.IncludeTag = new HashSet<string>(text);
        }

        private void CultureSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AkaI18N.Reload((I18NOption) CultureSelector.SelectedItem);
        }

        private async void DownloadPathTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsOpen)
                return;
            var caretIndex = DownloadPathTextBox.CaretIndex;
            var text = DownloadPathTextBox.Text;
            var beforeCaret = text.Substring(0, caretIndex);
            var lastIndex = beforeCaret.LastIndexOf("{", StringComparison.Ordinal);
            if (lastIndex != -1)
            {
                var filtered = await Task.Run(() =>
                {
                    var macros = DownloadPathMacros.GetEscapedMacros();
                    var substringFromFirstLeftCurlyBrace = beforeCaret.Substring(lastIndex);
                    return macros.Where(p => p.Macro.StartsWith(substringFromFirstLeftCurlyBrace)).ToArray();
                });
                if (!filtered.IsNullOrEmpty())
                {
                    MacroCompletionPopup.OpenControl();
                    UiHelper.NewItemsSource<DownloadPathMacros.MacroToken>(MacroCompletionBox).AddRange(filtered);
                }
                else
                {
                    MacroCompletionPopup.CloseControl();
                }
            }
        }

        private void MacroCompletionBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var token = (DownloadPathMacros.MacroToken) (sender as ListBox)!.SelectedItem;
            if (token == null)
                return;
            var caretIndex = DownloadPathTextBox.CaretIndex;
            var text = DownloadPathTextBox.Text;
            var beforeCaret = text.Substring(0, caretIndex);
            var afterCaret = text.Replace(beforeCaret, string.Empty);
            var lastIndexBeforeCaret = beforeCaret.LastIndexOf("{", StringComparison.Ordinal);
            var substringFromFirstLeftCurlyBrace = beforeCaret.Substring(lastIndexBeforeCaret);
            DownloadPathTextBox.Text = beforeCaret
                + token.Macro.Replace(substringFromFirstLeftCurlyBrace, string.Empty)
                + (token.IsConditional
                    ? afterCaret.RemoveUntil('}', afterCaret.Substring(0, afterCaret.IndexOf('}') is { } i ? i == -1 ? 0 : i : 0).Count(c => c == '{') + 1, '\\')
                    : token.Macro.RemoveCommonSubstringFromEnd(afterCaret));
            var newText = DownloadPathTextBox.Text;
            MacroCompletionPopup.CloseControl();
            DownloadPathTextBox.Focus();
            DownloadPathTextBox.CaretIndex = beforeCaret.Length + newText.Substring(beforeCaret.Length).IndexOf("}", StringComparison.Ordinal);
        }
    }
}