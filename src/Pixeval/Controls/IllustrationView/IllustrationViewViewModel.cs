#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationViewViewModel.cs
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

using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Pixeval.CoreApi.Model;
using Pixeval.Controls.Illustrate;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Controls.IllustrationView;

public sealed partial class IllustrationViewViewModel : SortableIllustrateViewViewModel<Illustration, IllustrationViewModel>
{
    [ObservableProperty]
    private bool _isSelecting;

    public override IllustrationViewDataProvider DataProvider { get; }

    public IllustrationViewViewModel(IllustrationViewViewModel viewModel)
    {
        DataProvider = viewModel.DataProvider.CloneRef();
        Init();
    }

    public IllustrationViewViewModel()
    {
        DataProvider = new IllustrationViewDataProvider();
        Init();
    }

    private void Init()
    {
        SelectionLabel = IllustrationViewCommandBarResources.CancelSelectionButtonDefaultLabel;
        DataProvider.SelectedIllustrations.CollectionChanged += (_, _) =>
        {
            IsAnyIllustrationSelected = DataProvider.SelectedIllustrations.Count > 0;
            var count = DataProvider.SelectedIllustrations.Count;
            SelectionLabel = count is 0
                ? IllustrationViewCommandBarResources.CancelSelectionButtonDefaultLabel
                : IllustrationViewCommandBarResources.CancelSelectionButtonFormatted.Format(count);
        };
    }

    public override void Dispose()
    {
        DataProvider.FetchEngine?.Cancel();
        DataProvider.DisposeCurrent();
    }

    public override void SetSortDescription(SortDescription description)
    {
        if (!DataProvider.View.SortDescriptions.Any())
        {
            DataProvider.View.SortDescriptions.Add(description);
            return;
        }

        DataProvider.View.SortDescriptions[0] = description;
    }

    public override void ClearSortDescription()
    {
        DataProvider.View.SortDescriptions.Clear();
    }
}
