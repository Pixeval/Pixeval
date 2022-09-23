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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty]
    private bool _hasNoItems;

    private SoftwareBitmapSource? _webQrCodeSource;

    private ObservableCollection<IllustrationViewModel> _illustrations;

    public ObservableCollection<IllustrationViewModel> Illustrations
    {
        get => _illustrations;
        set
        {
            SetProperty(ref _illustrations, value);
            IllustrationsView.Source = _illustrations;
        }
    }

    public IllustrationGridViewModel()
    {
        SelectedIllustrations = new ObservableCollection<IllustrationViewModel>();
        _illustrations = new ObservableCollection<IllustrationViewModel>();
        IllustrationsView = new AdvancedCollectionView(Illustrations);
        _selectionLabel = IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel;
        _visualizationController = new IllustrationVisualizationController(this)
        {
            CollectionChanged = (_, args) =>
            {
                void OnIsSelectedChanged(object? sender, IllustrationViewModel model)
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
                    IsAnyIllustrationSelected = SelectedIllustrations.Count != 0;

                    var count = SelectedIllustrations.Count;
                    SelectionLabel = count == 0
                        ? IllustrationGridCommandBarResources.CancelSelectionButtonDefaultLabel
                        : IllustrationGridCommandBarResources.CancelSelectionButtonFormatted.Format(count);
                }

                switch (args)
                {
                    case { Action: NotifyCollectionChangedAction.Add }:
                        args.NewItems?.OfType<IllustrationViewModel>().ForEach(i => i.IsSelectedChanged += OnIsSelectedChanged);
                        break;
                    case { Action: NotifyCollectionChangedAction.Remove }:
                        args.OldItems?.OfType<IllustrationViewModel>().ForEach(i => i.IsSelectedChanged -= OnIsSelectedChanged);
                        break;
                }
            }
        };
    }

    public IFetchEngine<Illustration?>? FetchEngine { get; set; }

    public AdvancedCollectionView IllustrationsView { get; }

    public ObservableCollection<IllustrationViewModel> SelectedIllustrations { get; }

    // Use this to add illustrations to IllustrationGrid
    private readonly IllustrationVisualizationController _visualizationController;

    public void Dispose()
    {
        _visualizationController.FetchEngine?.Cancel();
        DisposeCurrent();
        GC.SuppressFinalize(this);
    }

    public void AddIllustrationViewModel(IllustrationViewModel viewModel)
    {
        Illustrations.Add(viewModel);
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

    public async Task ResetEngineAndFillAsync(IFetchEngine<Illustration?>? newEngine, int? itemLimit = null)
    {
        HasNoItems = !await _visualizationController.ResetAndFillAsync(newEngine, itemLimit);
    }

    public void DisposeCurrent()
    {
        foreach (var illustrationViewModel in Illustrations)
        {
            illustrationViewModel.Dispose();
        }

        SelectedIllustrations.Clear();
        Illustrations.Clear();
        IllustrationsView.Clear();
    }
}