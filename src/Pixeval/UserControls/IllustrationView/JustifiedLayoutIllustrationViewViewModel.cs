#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/JustifiedLayoutIllustrationViewModel.cs
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
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Popups;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.UserControls.IllustrationView;

public sealed class JustifiedLayoutIllustrationViewViewModel : IllustrationViewViewModel
{
    private SoftwareBitmapSource? _pixEzQrCodeSource;

    private SoftwareBitmapSource? _webQrCodeSource;

    public JustifiedLayoutIllustrationViewViewModel()
    {
        SelectionLabel = IllustrationViewCommandBarResources.CancelSelectionButtonDefaultLabel;
        DataProvider = new JustifiedLayoutIllustrationViewDataProvider();
        DataProvider.SelectedIllustrations.CollectionChanged += (_, _) =>
        {
            IsAnyIllustrationSelected = DataProvider.SelectedIllustrations.Count > 0;
            var count = DataProvider.SelectedIllustrations.Count;
            SelectionLabel = count == 0
                ? IllustrationViewCommandBarResources.CancelSelectionButtonDefaultLabel
                : IllustrationViewCommandBarResources.CancelSelectionButtonFormatted.Format(count);
        };
    }

    public override IIllustrationViewDataProvider DataProvider { get; set; }

    public async Task ShowQrCodeForIllustrationAsync(IllustrationViewModel model)
    {
        _webQrCodeSource = await UIHelper.GenerateQrCodeForUrlAsync(MakoHelper.GenerateIllustrationWebUri(model.Id).ToString());

        PopupManager.ShowPopup(PopupManager.CreatePopup(new QrCodePresenter(_webQrCodeSource), lightDismiss: true, closing: (_, _) => _webQrCodeSource.Dispose()));
    }

    public async Task ShowPixEzQrCodeForIllustrationAsync(IllustrationViewModel model)
    {
        _pixEzQrCodeSource = await UIHelper.GenerateQrCodeAsync(MakoHelper.GenerateIllustrationPixEzUri(model.Id).ToString());

        PopupManager.ShowPopup(PopupManager.CreatePopup(new QrCodePresenter(_pixEzQrCodeSource), lightDismiss: true, closing: (_, _) => _pixEzQrCodeSource.Dispose()));
    }

    public override void Dispose()
    {
        DataProvider.FetchEngine?.Cancel();
        DataProvider.DisposeCurrent();
        GC.SuppressFinalize(this);
    }
}