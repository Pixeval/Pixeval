using Pixeval.Download.MacroParser;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Download
{
    public class IllustrationMetaPathParser : IMetaPathParser<IllustrationViewModel>
    {
        private readonly MacroParser<IllustrationViewModel> _parser = new();

        public IMetaPathMacroProvider<IllustrationViewModel> MacroProvider { get; }

        public IllustrationMetaPathParser()
        {
            MacroProvider = new IllustrationMetaPathMacroProvider();
        }

        public string Reduce(string raw, IllustrationViewModel context)
        {
            _parser.SetupParsingEnvironment(new Lexer(raw));
            if (_parser.Parse() is { } root)
            {
                var result = root.Evaluate(MacroProvider, context);
                return result.IsNotNullOrBlank() ? result : throw new MacroParseException(MacroParserResources.ResultIsEmpty);
            }

            throw new MacroParseException(MacroParserResources.ResultIsEmpty);
        }
    }
}