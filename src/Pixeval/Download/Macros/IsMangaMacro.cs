using Pixeval.Download.MacroParser;
using Pixeval.Util;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class IsMangaMacro : IMacro<IllustrationViewModel>.IPredicate
    {
        public string Name => "is_manga";

        public bool Match(IllustrationViewModel context)
        {
            return context.Illustration.IsManga();
        }
    }
}