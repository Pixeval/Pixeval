// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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
using Pixeval.Objects;
using Refit;
using Visibility = System.Windows.Visibility;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for SauceNAOHomePage.xaml
    /// </summary>
    public partial class SauceNAOHomePage
    {
        public SauceNAOHomePage()
        {
            InitializeComponent();
        }

        private async void UploadFileAndQueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            using var fileDialog = new CommonOpenFileDialog("选择文件")
            {
                Multiselect = false
            };
            try
            {
                if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Searching.Visibility = Visibility.Visible;
                    UploadFileTextBox.Text = fileDialog.FileName;
                    await using var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(UploadFileTextBox.Text), false);
                    var sauceResponse = await RestService.For<ISauceNAOProtocol>(ProtocolBase.SauceNAOUrl)
                        .GetSauce(new StreamPart(memoryStream, Path.GetFileName(fileDialog.FileName), Texts.AssumeImageContentType(fileDialog.FileName)));
                    var content = await sauceResponse.Content.ReadAsStringAsync();
                    if ((await ParseSauce(content)).ToList() is { } sauceResults && sauceResults.Any())
                        MainWindow.Instance.OpenIllustBrowser(await PixivHelper.IllustrationInfo(sauceResults[0]));
                    else MainWindow.MessageQueue.Enqueue("找不到结果TAT");
                }
            }
            finally
            {
                Searching.Visibility = Visibility.Hidden;
            }
        }

        private static async Task<IEnumerable<string>> ParseSauce(string sauceResult)
        {
            var doc = await new HtmlParser().ParseDocumentAsync(sauceResult);
            var results = doc.QuerySelectorAll("div.result")
                .Where(div => div.Attributes.Length == 1)
                .Where(div => div.ClassName == "result")
                .Select(div => div.QuerySelectorAll(".resultcontentcolumn"))
                .Where(i => i.Length == 1)
                .Select(i => i[0])
                .Select(i => i.QuerySelectorAll("div > a"));
            return results.Select(r => r[0].Text());
        }
    }
}