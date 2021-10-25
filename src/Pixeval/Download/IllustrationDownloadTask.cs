#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationDownloadTask.cs
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

using Pixeval.Options;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;

namespace Pixeval.Download
{
    public class IllustrationDownloadTask : ObservableDownloadTask
    {
        public IllustrationViewModel IllustrationViewModel { get; }

        public IllustrationDownloadTask(IllustrationViewModel illustrationViewModel, string destination) 
            : base(illustrationViewModel.Illustration.Title, illustrationViewModel.Illustration.User?.Name, illustrationViewModel.Illustration.GetOriginalUrl()!, IOHelper.NormalizePath(destination), illustrationViewModel.Illustration.GetThumbnailUrl(ThumbnailUrlOption.SquareMedium))
        {
            IllustrationViewModel = illustrationViewModel;
            CurrentState = DownloadState.Created;
        }
    }
}