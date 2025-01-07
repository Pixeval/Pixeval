// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Markup;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Pixeval.Controls;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public partial class ResourceStringExtension : MarkupExtension
{
    public string ResourceFile { get; set; } = "";

    public string ResourceKey { get; set; } = "";

    protected override object ProvideValue()
    {
        var resourceLoader = new ResourceLoader(ResourceLoader.GetDefaultResourceFilePath(), ResourceFile);
        return resourceLoader.GetString(ResourceKey);
    }
}
