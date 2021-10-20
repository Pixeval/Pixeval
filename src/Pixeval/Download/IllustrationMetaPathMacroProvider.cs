using System.Collections.Generic;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.ViewModel;

namespace Pixeval.Download
{
    public class IllustrationMetaPathMacroProvider : IMetaPathMacroProvider<IllustrationViewModel>
    {
        public IReadOnlyList<IMacro<IllustrationViewModel>> AvailableMacros { get; } = new List<IMacro<IllustrationViewModel>>
        {
            new FileExtensionMacro(),
            new IllustrationNameMacro(),
            new IllustratorNameMacro(),
            new IsFromSpotlightMacro(),
            new IsMangaMacro(),
            new IsUgoiraMacro(),
            new MangaIndexMacro(),
            new SpotlightIdMacro(),
            new SpotlightTitleMacro()
        };
    }
}