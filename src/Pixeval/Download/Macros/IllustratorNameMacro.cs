using Pixeval.Download.MacroParser;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class IllustratorNameMacro : IMacro<IllustrationViewModel>.ITransducer
    {
        public string Name => "artist_name";

        public string Substitute(IllustrationViewModel context)
        {
            return context.Illustration.User?.Name ?? MacroParserResources.UnknownArtist;
        }
    }
}