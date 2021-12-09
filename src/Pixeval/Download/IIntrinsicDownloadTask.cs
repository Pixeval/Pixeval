#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IIntrinsicDownloadTask.cs
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

using Windows.Storage.Streams;

namespace Pixeval.Download;

/// <summary>
/// An <see cref="IIntrinsicDownloadTask"/> is a special kind of <see cref="IDownloadTask"/> that will manually
/// take care of the stream of the downloaded content, meanwhile, <see cref="IIntrinsicDownloadTask"/> will not
/// be recorded by the <see cref="DownloadManager{TDownloadTask}"/>, such tasks are mostly used when the content
/// is already present.
/// </summary>
public interface IIntrinsicDownloadTask : IDownloadTask
{
    IRandomAccessStream Stream { get; }
}