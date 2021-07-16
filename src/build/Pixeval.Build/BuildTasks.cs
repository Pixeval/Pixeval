using System;
using System.IO;
using System.IO.Compression;

namespace Pixeval.Build
{
    public class BuildTask : Attribute
    {
        public BuildTask(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public static class BuildTasks
    {
        [BuildTask("Copy-LoginProxy-PixevalAssets")]
        public static void ZipAndCopyLoginProxyToPixevalAssets()
        {
            var src = FileHelper.FindSrc();
            var publishFolder = Path.Combine(src, @"Pixeval.LoginProxy\bin\Release\net5.0-windows\publish");
            var assets = Path.Combine(src, @"Pixeval\Assets\Binary\Pixeval.LoginProxy.zip");
            if (File.Exists(assets))
            {
                File.Delete(assets);
            }
            if (!Directory.Exists(Directory.GetParent(assets)!.FullName))
            {
                Directory.CreateDirectory(Directory.GetParent(assets)!.FullName);
            }
            ZipFile.CreateFromDirectory(publishFolder, assets, CompressionLevel.Optimal, false);
        }
    }
}