#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/LazyInitializedAnimatedIllustrationDownloadTask.cs
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

using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.Database;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class LazyInitializedUgoiraDownloadTask(DownloadHistoryEntry databaseEntry) : UgoiraDownloadTask(databaseEntry, null!, null!), ILazyLoadDownloadTask
{
    public override async Task DownloadAsync(Downloader downloadStreamAsync)
    {
        await LazyLoadAsync(DatabaseEntry.Entry);

        await base.DownloadAsync(downloadStreamAsync);
    }

    public async Task LazyLoadAsync(IWorkEntry workEntry)
    {
        if (workEntry is not Illustration illustration)
        {
            ThrowHelper.Argument(workEntry);
            return;
        }
        IllustrationViewModel ??= new(illustration);
        Metadata ??= await IllustrationViewModel.UgoiraMetadata.ValueAsync;
    }
}
