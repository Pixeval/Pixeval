using Pixeval.Download.MacroParser;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class IsFromSpotlightMacro : IMacro<IllustrationViewModel>.IPredicate
    {
        public string Name => "is_spot";

        public bool Match(IllustrationViewModel context)
        {
            return context.Illustration.FromSpotlight;
        }
    }
}