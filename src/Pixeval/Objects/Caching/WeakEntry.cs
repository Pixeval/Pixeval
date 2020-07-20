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

using System;
using System.Runtime.InteropServices;

#nullable enable

namespace Pixeval.Objects.Caching
{
    public class WeakEntry<T> : IEquatable<WeakEntry<T>>, IDisposable where T : class
    {
        private readonly int hashCode;

        private GCHandle gcHandle;

        public WeakEntry(T target)
        {
            hashCode = target.GetHashCode();
            gcHandle = GCHandle.Alloc(target, GCHandleType.Weak);
        }

        public bool IsAlive => gcHandle.Target != null;

        public T? Target => gcHandle.Target as T;

        public void Dispose()
        {
            gcHandle.Free();
            GC.SuppressFinalize(this);
        }

        public bool Equals(WeakEntry<T>? other)
        {
            return other != null && ReferenceEquals(other.Target, Target);
        }

        ~WeakEntry()
        {
            Dispose();
        }

        public override bool Equals(object? obj)
        {
            if (obj is WeakEntry<T> weakEntry) return Equals(weakEntry);
            return false;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}
