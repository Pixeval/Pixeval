using Pixeval.Download.MacroParser;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class SpotlightIdMacro : IMacro<IllustrationViewModel>.ITransducer
    {
        public string Name => "spot_id";

        public string Substitute(IllustrationViewModel context)
        {
            return context.Illustration.SpotlightId ?? MacroParserResources.UnknownSpotlightId;
        }
    }
}