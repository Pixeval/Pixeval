// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Extensions.Common;

namespace Pixeval.Extensions;

public record ExtensionsHostModel(IExtensionsHost Host)
{
    public string Name { get; } = Host.GetExtensionName();

    public string Description { get; } = Host.GetDescription();

    public string Author { get; } = Host.GetAuthorName();

    public string Version { get; } = Host.GetVersion();

    public string Link { get; } = Host.GetExtensionLink();

    public string HelpLink { get; } = Host.GetHelpLink();

    public IReadOnlyList<IExtension> Extensions { get; } = Host.GetExtensions();
}
