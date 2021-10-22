#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MetaPathMacroAttribute.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pixeval.Download.Macros
{
    public class MetaPathMacroAttribute : Attribute
    {
        public object Key;

        public MetaPathMacroAttribute(object key)
        {
            Key = key;
        }
    }

    public static class MetaPathMacroAttributeHelper
    {
        public static IEnumerable<T> GetAttachedTypeInstances<T>()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(it => it.GetCustomAttribute<MetaPathMacroAttribute>() is not null)
                .Select(Activator.CreateInstance)
                .OfType<T>();
        }

        public static IEnumerable<T> GetAttachedTypeInstances<T>(object key)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(it => it.GetCustomAttribute<MetaPathMacroAttribute>() is { Key: var attrKey } && attrKey.Equals(key))
                .Select(Activator.CreateInstance)
                .OfType<T>();
        }
    }
}