using Pixeval.Download.MacroParser;
using Pixeval.ViewModel;

namespace Pixeval.Download.Macros
{
    public class MangaIndexMacro : IMacro<IllustrationViewModel>.ITransducer
    {
        public string Name => "manga_index";

        public string Substitute(IllustrationViewModel context)
        {
            return context.MangaIndex.ToString();
        }
    }
}