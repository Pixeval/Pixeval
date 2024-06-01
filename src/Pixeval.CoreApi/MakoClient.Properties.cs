#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/MakoClient.Properties.cs
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
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Preference;
using Pixeval.Logging;

namespace Pixeval.CoreApi;

public partial class MakoClient
{
    private readonly List<IEngineHandleSource> _runningInstances = [];

    /// <summary>
    /// The globally unique ID of current <see cref="MakoClient" />
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    public Session Session { get; private set; }

    public MakoClientConfiguration Configuration { get; set; }

    public FileLogger Logger { get; }

    /// <summary>
    /// The IoC container
    /// </summary>
    internal ServiceCollection Services { get; } = [];

    internal ServiceProvider Provider { get; }

    public bool IsCancelled { get; set; }
}
