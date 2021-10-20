using Pixeval.Download.MacroParser;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class IsUgoiraMacro : IMacro<IllustrationViewModel>.IPredicate
    {
        public string Name => "is_gif";

        public bool Match(IllustrationViewModel context)
        {
            return context.Illustration.IsUgoira();
        }
    }
}