#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IllustrationGridViewModel.cs
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
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Popups;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.UserControls;

public partial class IllustrationGridViewModel : ObservableObject, IDisposable
{
    private SoftwareBitmapSource? _pixEzQrCodeSource;

    [ObservableProperty]
    private bool _isAnyIllustrationSelected;

    [ObservableProperty]
    private string _selectionLabel;

    [ObservableProperty]
    private bool _hasNoItems;

    private SoftwareBitmapSource? _webQrCodeSource;

    public IIllustrationViewDataProvider DataProvider { get; set; }

    public IllustrationGridViewModel()
    {
        _selectionLabel = IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel;
        DataProvider = new GridIllustrationViewDataProvider();
        DataProvider.SelectedIllustrations.CollectionChanged += (_, _) =>
        {
            IsAnyIllustrationSelected = DataProvider.SelectedIllustrations.Count > 0;
            var count = DataProvider.SelectedIllustrations.Count;
            SelectionLabel = count == 0
                ? IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel
                : IllustrationGridCommandBarResources.CancelSelectionButtonFormatted.Format(count);
        };
    }

    public void Dispose()
    {
        DataProvider.FetchEngine?.Cancel();
        DisposeCurrent();
        GC.SuppressFinalize(this);
    }

    public void SetSortDescription(SortDescription description)
    {
        if (!DataProvider.IllustrationsView.SortDescriptions.Any())
        {
            DataProvider.IllustrationsView.SortDescriptions.Add(description);
            return;
        }

        DataProvider.IllustrationsView.SortDescriptions[0] = description;
    }

    public void ClearSortDescription()
    {
        DataProvider.IllustrationsView.SortDescriptions.Clear();
    }

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

    public async Task ResetEngineAndFillAsync(IFetchEngine<Illustration?>? newEngine, int? itemLimit = null)
    {
        HasNoItems = !await DataProvider.ResetAndFillAsync(newEngine, itemLimit);
    }

    public void DisposeCurrent()
    {
        DataProvider.DisposeCurrent();
    }
}