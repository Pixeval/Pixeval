// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Windows.Foundation.Collections;
using Microsoft.Windows.Storage;
using Pixeval.Extensions.Common;

namespace Pixeval.Extensions;

public record ExtensionsHostModel(IExtensionsHost Host)
{
    public bool IsActive
    {
        get
        {
            if (Values.TryGetValue(nameof(IsActive), out var value) && value is bool v)
                return v;

            Values[nameof(IsActive)] = true;
            return true;
        }
        set => Values[nameof(IsActive)] = value;
    }

    public string Name { get; } = Host.GetExtensionName();

    public string Description { get; } = Host.GetDescription();

    public string Author { get; } = Host.GetAuthorName();

    public string Version { get; } = Host.GetVersion();

    public string Link { get; } = Host.GetExtensionLink();

    public string HelpLink { get; } = Host.GetHelpLink();

    public IPropertySet Values { get; } = GetValues(Host);

    public IReadOnlyList<IExtension> Extensions { get; } = Host.GetExtensions();

    private static IPropertySet GetValues(IExtensionsHost host)
    {
        var localSettings = ApplicationData.GetDefault().LocalSettings;
        var extensionName = host.GetExtensionName();
        if (!localSettings.Containers.TryGetValue(extensionName, out var container))
            container = localSettings.CreateContainer(extensionName, ApplicationDataCreateDisposition.Always);
        return container.Values;
    }
}
