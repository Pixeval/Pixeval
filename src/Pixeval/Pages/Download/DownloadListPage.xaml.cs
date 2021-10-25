#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/DownloadListPage.xaml.cs
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Download;
using Pixeval.Util.UI;

namespace Pixeval.Pages.Download
{
    public sealed partial class DownloadListPage
    {
        private DownloadListPageViewModel _viewModel = null!;

        public DownloadListPage()
        {
            InitializeComponent();
        }

        public override void OnPageActivated(NavigationEventArgs e)
        {
            _viewModel = new DownloadListPageViewModel(((IEnumerable<ObservableDownloadTask>) e.Parameter).Select(o => new DownloadListEntryViewModel(o)).ToList());
        }

        public override void OnPageDeactivated(NavigatingCancelEventArgs e)
        {
            foreach (var downloadListEntryViewModel in _viewModel.DownloadTasks)
            {
                downloadListEntryViewModel.Dispose();
            }
        }

        private void ModeFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.ResetFilter();
        }
    }
}