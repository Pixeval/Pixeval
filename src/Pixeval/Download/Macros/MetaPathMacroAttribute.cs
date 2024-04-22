#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/MetaPathMacroAttribute.cs
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
using Pixeval.Download.MacroParser;

namespace Pixeval.Download.Macros;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MetaPathMacroAttribute<TContext> : Attribute;

public static class MetaPathMacroAttributeHelper
{
    public static IEnumerable<IMacro> GetAttachedTypeInstances()
    {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetCustomAttribute(typeof(MetaPathMacroAttribute<>))?.GetType() is not null)
            .Select(Activator.CreateInstance)
            .OfType<IMacro>();
    }

    public static IEnumerable<IMacro> GetAttachedTypeInstances<TContext>()
    {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(t =>
                t.GetCustomAttribute(typeof(MetaPathMacroAttribute<>))?.GetType()
                    .GenericTypeArguments is [var type] && type.IsAssignableFrom(typeof(TContext)))
            .Select(Activator.CreateInstance)
            .OfType<IMacro>();
    }
}
