// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
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

    public TokenUser Me => Provider.GetRequiredService<PixivTokenProvider>().Me;

    public MakoClientConfiguration Configuration { get; set; }

    public FileLogger Logger { get; }

    /// <summary>
    /// The IoC container
    /// </summary>
    internal ServiceCollection Services { get; } = [];

    internal ServiceProvider Provider { get; }

    public bool IsCancelled { get; set; }
}
