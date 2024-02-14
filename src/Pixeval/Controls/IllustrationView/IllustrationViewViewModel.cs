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

using Pixeval.Controls.Illustrate;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public sealed class IllustrationViewViewModel : SortableIllustrateViewViewModel<Illustration, IllustrationItemViewModel>
{
    private IllustrationItemViewModel[] _selectedIllustrations = [];

    public IllustrationViewViewModel(IllustrationViewViewModel viewModel)
    {
        DataProvider = viewModel.DataProvider.CloneRef();
        SelectionLabel = IllustrationViewCommandBarResources.CancelSelectionButtonDefaultLabel;
    }

    public IllustrationViewViewModel()
    {
        DataProvider = new();
        SelectionLabel = IllustrationViewCommandBarResources.CancelSelectionButtonDefaultLabel;
    }

    public override IllustrationViewDataProvider DataProvider { get; }

    public override IllustrationItemViewModel[] SelectedIllustrations
    {
        get => _selectedIllustrations;
        set
        {
            if (Equals(value, _selectedIllustrations))
                return;
            _selectedIllustrations = value;
            var count = value.Length;
            IsAnyIllustrationSelected = count > 0;
            SelectionLabel = IsAnyIllustrationSelected
                ? IllustrationViewCommandBarResources.CancelSelectionButtonFormatted.Format(count)
                : IllustrationViewCommandBarResources.CancelSelectionButtonDefaultLabel;
            OnPropertyChanged();
        }
    }
}
