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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pixeval.Core;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Protocol;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Refit;
using Visibility = System.Windows.Visibility;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for SauceNAOHomePage.xaml
    /// </summary>
    public partial class SauceNaoHomePage
    {
        public SauceNaoHomePage()
        {
            InitializeComponent();
        }

        private async void SauceNaoPage_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] fs)
                {
                    if (fs.Length > 1)
                    {
                        MainWindow.MessageQueue.Enqueue(AkaI18N.SauceNaoFileCountLimit);
                    }
                    else
                    {
                        try
                        {
                            UploadFileAndQueryButton.IsEnabled = false;
                            Searching.Visibility = Visibility.Visible;
                            UploadFileTextBox.Text = fs[0];
                            if ((await DoQuery(fs[0])).ToList() is { } sauceResults && sauceResults.Any())
                            {
                                MainWindow.Instance.OpenIllustBrowser(await PixivHelper.IllustrationInfo(sauceResults[0]));
                            }
                            else
                            {
                                MainWindow.MessageQueue.Enqueue(AkaI18N.CannotFindResult);
                            }
                        }
                        finally
                        {
                            UploadFileAndQueryButton.IsEnabled = true;
                            Searching.Visibility = Visibility.Hidden;
                            UploadFileTextBox.Text = "";
                        }
                    }
                }
            }
        }

        private async void UploadFileAndQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            using var fileDialog = new CommonOpenFileDialog(AkaI18N.PleaseSelectFile) { Multiselect = false };
            try
            {
                if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    UploadFileAndQueryButton.IsEnabled = false;
                    Searching.Visibility = Visibility.Visible;
                    UploadFileTextBox.Text = fileDialog.FileName;
                    if ((await DoQuery(fileDialog.FileName)).ToList() is { } sauceResults && sauceResults.Any())
                    {
                        MainWindow.Instance.OpenIllustBrowser(await PixivHelper.IllustrationInfo(sauceResults[0]));
                    }
                    else
                    {
                        MainWindow.MessageQueue.Enqueue(AkaI18N.CannotFindResult);
                    }
                }
            }
            finally
            {
                UploadFileAndQueryButton.IsEnabled = true;
                Searching.Visibility = Visibility.Hidden;
                UploadFileTextBox.Text = "";
            }
        }

        private async Task<IEnumerable<string>> DoQuery(string fileName)
        {
            await using var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(UploadFileTextBox.Text), false);
            var sauceResponse = await RestService.For<ISauceNAOProtocol>(ProtocolBase.SauceNaoUrl).GetSauce(new StreamPart(memoryStream, Path.GetFileName(fileName), Strings.AssumeImageContentType(fileName)));
            var content = await sauceResponse.Content.ReadAsStringAsync();
            return await ParseSauce(content);
        }

        private static async Task<IEnumerable<string>> ParseSauce(string sauceResult)
        {
            var doc = await new HtmlParser().ParseDocumentAsync(sauceResult);
            var results = doc.QuerySelectorAll("div.result").Where(div => div.Attributes.Length == 1).Where(div => div.ClassName == "result").Select(div => div.QuerySelectorAll(".resultcontentcolumn")).Where(i => i.Length == 1).Select(i => i[0]).Select(i => i.QuerySelectorAll("div > a"));
            return results.Where(r => r.Any()).Select(r => r[0].Text());
        }
    }
}