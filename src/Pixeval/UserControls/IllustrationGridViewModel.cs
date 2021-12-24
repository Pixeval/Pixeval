#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationGridViewModel.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Popups;
using Pixeval.Util;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.UserControls;

public partial class IllustrationGridViewModel : ObservableObject, IDisposable, IIllustrationVisualizer
{
    [ObservableProperty]
    private bool _isAnyIllustrationSelected;

    private SoftwareBitmapSource? _pixEzQrCodeSource;

    [ObservableProperty]
    private string _selectionLabel;

    private SoftwareBitmapSource? _webQrCodeSource;

    public IllustrationGridViewModel()
    {
        SelectedIllustrations = new ObservableCollection<IllustrationViewModel>();
        Illustrations = new ObservableCollection<IllustrationViewModel>();
        IllustrationsView = new AdvancedCollectionView(Illustrations);
        _selectionLabel = IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel;
        VisualizationController = new IllustrationVisualizationController(this);
    }

    public IFetchEngine<Illustration?>? FetchEngine { get; set; }

    public ObservableCollection<IllustrationViewModel> Illustrations { get; }

    public AdvancedCollectionView IllustrationsView { get; }

    public ObservableCollection<IllustrationViewModel> SelectedIllustrations { get; }

    public IllustrationVisualizationController VisualizationController { get; internal set; }

    public void Dispose()
    {
        DisposeCurrent();
        GC.SuppressFinalize(this);
    }

    public void AddIllustrationViewModel(IllustrationViewModel viewModel)
    {
        viewModel.OnIsSelectedChanged += (_, model) => // add/remove the viewModel to/from SelectedIllustrations according to the IsSelected Property
        {
            if (model.IsSelected)
            {
                SelectedIllustrations.Add(model);
            }
            else
            {
                SelectedIllustrations.Remove(model);
            }

            // Update the IsAnyIllustrationSelected Property if any of the viewModel's IsSelected property changes
            IsAnyIllustrationSelected = SelectedIllustrations.Any();

            var count = SelectedIllustrations.Count;
            SelectionLabel = count == 0
                ? IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel
                : IllustrationGridCommandBarResources.CancelSelectionButtonFormatted.Format(count);
        };
        IllustrationsView.Add(viewModel);
    }

    public void SetSortDescription(SortDescription description)
    {
        if (!IllustrationsView.SortDescriptions.Any())
        {
            IllustrationsView.SortDescriptions.Add(description);
            return;
        }

        IllustrationsView.SortDescriptions[0] = description;
    }

    public void ClearSortDescription()
    {
        IllustrationsView.SortDescriptions.Clear();
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

    public void DisposeCurrent()
    {
        foreach (IllustrationViewModel illustrationViewModel in IllustrationsView)
        {
            illustrationViewModel.Dispose();
        }

        SelectedIllustrations.Clear();
        IllustrationsView.Clear();
    }

    ~IllustrationGridViewModel()
    {
        Dispose();
    }
}