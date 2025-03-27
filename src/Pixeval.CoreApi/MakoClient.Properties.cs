// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Mako.Engine;
using Mako.Model;
using Mako.Net;
using Mako.Preference;
using Pixeval.Logging;

namespace Mako;

public partial class MakoClient
{
    private readonly List<IEngineHandleSource> _runningInstances = [];

    /// <summary>
    /// The globally unique ID of current <see cref="MakoClient" />
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    public TokenUser Me => Provider.GetRequiredService<PixivTokenProvider>().Me;

    public TokenUser? TryGetMe() => Provider.GetService<PixivTokenProvider>()?.Me;

    public MakoClientConfiguration Configuration { get; set; }

    public FileLogger Logger { get; }

    /// <summary>
    /// The IoC container
    /// </summary>
    internal ServiceCollection Services { get; } = [];

    internal ServiceProvider Provider { get; }

    public bool IsCancelled { get; set; }
}
