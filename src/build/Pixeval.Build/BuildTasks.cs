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
            var dest = Path.Combine(Path.GetTempPath(), "Pixeval.LoginProxy.zip");
            var assets = Path.Combine(src, @"Pixeval\Assets\Binary\Pixeval.LoginProxy.zip");
            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
            ZipFile.CreateFromDirectory(publishFolder, dest, CompressionLevel.Optimal, false);
            if (!Directory.Exists(Directory.GetParent(assets)!.FullName))
            {
                Directory.CreateDirectory(Directory.GetParent(assets)!.FullName);
            }
            File.Copy(dest, assets, true);
            File.Delete(dest);
        }
    }
}