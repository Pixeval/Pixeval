using System;
using System.IO;
using System.Reflection;

namespace Pixeval
{
    /// <summary>
    /// Provide miscellaneous information about the app
    /// </summary>
    public static class AppContext
    {
        public const string AppIdentifier = "Pixeval";

        public static string AppConfigurationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppIdentifier);

        public static string AppSessionFileName = Path.Combine(AppConfigurationFolder, "Session.json");

        public static string AppConfigurationFileName = Path.Combine(AppConfigurationFolder, "Settings.json");

        public static string AppFolder => Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName;
    }
}