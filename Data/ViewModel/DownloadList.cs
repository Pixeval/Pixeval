// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pixeval.Objects;

namespace Pixeval.Data.ViewModel
{
    internal class DownloadList
    {
        public static ObservableCollection<Illustration> ToDownloadList { get; } = new ObservableCollection<Illustration>();

        public static void Add(Illustration illustration)
        {
            if (ToDownloadList.All(x => x.Id != illustration.Id)) ToDownloadList.Add(illustration);
        }

        public static void AddRange(IEnumerable<Illustration> illustrations)
        {
            ToDownloadList.AddRange(illustrations.Where(x => ToDownloadList.All(i => i.Id != x.Id)));
        }

        public static void Remove(Illustration illustration)
        {
            ToDownloadList.Remove(illustration);
        }
    }
}