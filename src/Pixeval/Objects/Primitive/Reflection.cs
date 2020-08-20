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
using System.Reflection;
using Pixeval.Wpf.Objects.Exceptions;

namespace Pixeval.Wpf.Objects.Primitive
{
    public static class Reflection
    {
        public static T GetCustomAttribute<T>(this object obj) where T : Attribute
        {
            return CustomAttributeExtensions.GetCustomAttribute<T>(obj.GetType()) ??
                throw new AttributeNotFoundException(typeof(T).ToString());
        }

        public static T GetEnumAttribute<T>(this Enum value) where T : Attribute
        {
            return value.GetType().GetField(value.ToString())!.GetCustomAttribute<T>(false) ??
                throw new AttributeNotFoundException(typeof(T).ToString());
        }

        public static bool AttributeAttached<T>(this Enum value) where T : Attribute
        {
            return value.GetType().GetField(value.ToString())!.GetCustomAttribute<T>(false) != null;
        }
    }
}
