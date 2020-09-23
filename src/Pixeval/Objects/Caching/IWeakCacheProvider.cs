#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
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

#endregion

using System.Threading.Tasks;

namespace Pixeval.Objects.Caching
{
    public interface IWeakCacheProvider<T, in THash> where T : class
    {
        void Attach(ref T key, THash associateWith);

        void Detach(THash associateWith);

        Task<(bool, T)> TryGet(THash key);

        void Clear();

        protected static int HashKey(THash key)
        {
            return key == null ? 0 : key.GetHashCode();
        }
    }
}