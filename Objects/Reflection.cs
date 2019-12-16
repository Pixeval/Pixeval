// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System;
using System.Linq;
using System.Reflection;

namespace Pixeval.Objects
{
    public static class Reflection
    {
        public static MethodInfo[] PublicInstanceMethods<T>()
        {
            return typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public);
        }

        public static bool AttributeAttached<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttribute<T>() != null;
        }

        public static bool IsImplementOf<TDerived, TInterface>()
        {
            return typeof(TDerived).IsImplementOf(typeof(TInterface));
        }

        public static bool IsImplementOf(this Type derivedType, Type interfaceType)
        {
            return interfaceType.IsAssignableFrom(derivedType);
        }

        public static T NewInstance<T>()
        {
            return (T) NewInstance(typeof(T));
        }

        public static object NewInstance(Type type)
        {
            return type.GetConstructors().First(ctor => !ctor.GetParameters().Any()).Invoke(new object[] { });
        }
    }
}