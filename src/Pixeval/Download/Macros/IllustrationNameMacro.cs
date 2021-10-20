using Pixeval.Download.MacroParser;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class IllustrationNameMacro : IMacro<IllustrationViewModel>.ITransducer
    {
        public string Name => "illust_name";

        public string Substitute(IllustrationViewModel context)
        {
            return context.Illustration.Title ?? MacroParserResources.UnknownIllustrationTitle;
        }
    }
}