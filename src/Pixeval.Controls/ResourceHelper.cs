using System.IO;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Pixeval.Controls;

internal static class ResourceHelper
{
    public const string AssemblyName = $"{nameof(Pixeval)}.{nameof(Controls)}";

    public static ResourceLoader GetResourceLoader(string name)
        => new(Path.GetDirectoryName(ResourceLoader.GetDefaultResourceFilePath()) + @$"\{AssemblyName}.pri", $"ms-resource://{AssemblyName}/{AssemblyName}/{name}");
}
