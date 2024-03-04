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

using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public sealed partial class IllustrationViewViewModel : SortableIllustrateViewViewModel<Illustration, IllustrationItemViewModel>
{
    [ObservableProperty]
    private bool _isSelecting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnyIllustrationSelected))]
    [NotifyPropertyChangedFor(nameof(SelectionLabel))]
    private IllustrationItemViewModel[] _selectedIllustrations = [];

    public bool IsAnyIllustrationSelected => SelectedIllustrations.Length > 0;

    public string SelectionLabel => IsAnyIllustrationSelected
        ? IllustrationViewCommandBarResources.CancelSelectionButtonFormatted.Format(SelectedIllustrations.Length)
        : IllustrationViewCommandBarResources.CancelSelectionButtonDefaultLabel;

    public IllustrationViewViewModel(IllustrationViewViewModel viewModel) : this(viewModel.DataProvider.CloneRef())
    {
    }

    public IllustrationViewViewModel() : this(new IllustrationViewDataProvider())
    {
    }

    public IllustrationViewViewModel(IllustrationViewDataProvider dataProvider)
    {
        DataProvider = dataProvider;
        dataProvider.View.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoItem));
    }

    public override IllustrationViewDataProvider DataProvider { get; }
}
