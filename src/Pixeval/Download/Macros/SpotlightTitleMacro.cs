using Pixeval.Download.MacroParser;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class SpotlightTitleMacro : IMacro<IllustrationViewModel>.ITransducer
    {
        public string Name => "spot_title";

        public string Substitute(IllustrationViewModel context)
        {
            return context.Illustration.SpotlightTitle ?? MacroParserResources.UnknownSpotlightTitle;
        }
    }
}