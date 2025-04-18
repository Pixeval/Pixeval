// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Misaki;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.Utilities;

namespace Pixeval.Download;

public class ArtworkMetaPathParser : IMetaPathParser<IArtworkInfo>
{
    private ArtworkMetaPathParser()
    {
    }

    public static IMetaPathParser<IArtworkInfo> Instance { get; } = new ArtworkMetaPathParser();

    private readonly MacroParser<IArtworkInfo> _parser = new();

    public IReadOnlyList<IMacro> MacroProvider { get; } = MetaPathMacroAttributeHelper.GetIArtworkInfoInstances();

    public string Reduce(string raw, IArtworkInfo context)
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
