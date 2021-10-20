using System;
using Pixeval.Download.MacroParser;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class FileExtensionMacro : IMacro<IllustrationViewModel>.ITransducer
    {
        public string Name => "illust_ext";

        public string Substitute(IllustrationViewModel context)
        {
            if (context.Illustration.IsUgoira())
            {
                return ".gif";
            }

            return context.Illustration.GetOriginalUrl() is { } url ? url[url.LastIndexOf(".", StringComparison.Ordinal)..] : string.Empty;
        }
    }
}