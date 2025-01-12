// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.Utilities;

namespace Pixeval.Download;

public class IllustrationMetaPathParser : IMetaPathParser<IllustrationItemViewModel>
{
    private readonly MacroParser<IllustrationItemViewModel> _parser = new();

    private static readonly IReadOnlyList<IMacro> _MacroProviderStatic = MetaPathMacroAttributeHelper.GetIWorkViewModelInstances();
 
    public IReadOnlyList<IMacro> MacroProvider => _MacroProviderStatic;

    public string Reduce(string raw, IllustrationItemViewModel context)
    {
        _parser.SetupParsingEnvironment(new Lexer(raw));
        if (_parser.Parse() is { } root)
        {
            var result = root.Evaluate(MacroProvider, context);
            if (result.IsNotNullOrBlank())
                return result;
        }

        return ThrowUtils.MacroParse<string>(MacroParserResources.ResultIsEmpty);
    }
}
