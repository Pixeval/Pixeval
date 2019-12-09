using System;
using System.IO;

namespace Pixeval.Caching.Persisting
{
    public class PixevalEnvironment
    {
        public static readonly string ProjectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pixeval");

        public static readonly string ConfFolder = Path.Combine(ProjectFolder, "conf");

        public static readonly string TempFolder = Path.Combine(ProjectFolder, "tmp");
    }
}