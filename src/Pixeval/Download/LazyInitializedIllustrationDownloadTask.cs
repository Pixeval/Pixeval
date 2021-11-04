#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/LazyInitializedIllustrationDownloadTask.cs
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
using Pixeval.UserControls;

namespace Pixeval.Download
{
    public class LazyInitializedIllustrationDownloadTask : ObservableDownloadTask, IIllustrationViewModelProvider
    {
        private readonly Lazy<Task<IllustrationViewModel>> _resultGenerator;

        public LazyInitializedIllustrationDownloadTask(
            string illustId,
            string? title, 
            string? description, 
            string url, 
            string destination, 
            string? thumbnail,
            string? uniqueId, 
            DownloadState finalState)
            : base(title, description, url, destination, thumbnail, uniqueId)
        {
            CurrentState = finalState;
            _resultGenerator = new Lazy<Task<IllustrationViewModel>>(async () => new IllustrationViewModel(await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(illustId)));
        }

        public Task<IllustrationViewModel> GetViewModelAsync()
        {
            return _resultGenerator.Value;
        }
    }
}