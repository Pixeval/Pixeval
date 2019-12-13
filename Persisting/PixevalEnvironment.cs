using System;
using System.IO;

namespace Pixeval.Persisting
{
    public class PixevalEnvironment
    {
        public static readonly string ProjectFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pixeval");

        public static readonly string ConfFolder = ProjectFolder;

        public static readonly string TempFolder = Path.Combine(ProjectFolder, "tmp");

        public static readonly string SettingsFolder = ProjectFolder;

        internal static bool LogoutExit = false;

        static PixevalEnvironment()
        {
            if (!Directory.Exists(ProjectFolder)) Directory.CreateDirectory(ProjectFolder);

            if (!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);

            if (!Directory.Exists(SettingsFolder)) Directory.CreateDirectory(SettingsFolder);
        }
    }
}