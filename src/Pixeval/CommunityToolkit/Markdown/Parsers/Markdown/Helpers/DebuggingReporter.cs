using System.Diagnostics;

namespace Pixeval.CommunityToolkit.Markdown.Parsers.Markdown.Helpers
{
    /// <summary>
    /// Reports an error during debugging.
    /// </summary>
    internal class DebuggingReporter
    {
        /// <summary>
        /// Reports a critical error.
        /// </summary>
        public static void ReportCriticalError(string errorText)
        {
            Debug.WriteLine(errorText);
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }
    }
}