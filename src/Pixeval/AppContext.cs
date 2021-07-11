using System;
using System.IO;

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
    }
}