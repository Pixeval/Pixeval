// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Download.MacroParser;

public static class MetaPathMacroProviderHelper
{
    public static IMacro TryResolve(this IEnumerable<IMacro> availableMacros, string macro, bool isNot)
    {
        var m = availableMacros.FirstOrDefault(m => m.Name == macro) ?? new Unknown();
        if (m is IPredicate p) 
            p.IsNot = isNot;
        return m;
    }
}
