﻿#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ServiceCollectionHelper.cs
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

using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Utilities;

namespace Pixeval.Util;

public static class ServiceCollectionHelper
{
    public static void AddAllGenericTypes<T>(this IServiceCollection services, ServiceLifetime lifetime)
    {
        if (!typeof(T).IsGenericType)
        {
            return;
        }

        var types = Assembly.GetExecutingAssembly().DefinedTypes
            .Where(x => x.GetInterfaces().Any(i => i.IsGenericType
                                                   && i.GetGenericTypeDefinition() == typeof(T)
                                                   && i.GetGenericArguments().SequenceEquals(typeof(T).GetGenericArguments())));
        foreach (var typeInfo in types)
        {
            services.Add(new ServiceDescriptor(typeof(T), typeInfo, lifetime));
            services.Add(new ServiceDescriptor(typeInfo, typeInfo, lifetime));
        }
    }
}