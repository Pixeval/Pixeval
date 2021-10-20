using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Download.MacroParser
{
    public interface IMetaPathMacroProvider<TContext>
    {
        IReadOnlyList<IMacro<TContext>> AvailableMacros { get; }

        IMacro<TContext> TryResolve(string macro)
        {
            return AvailableMacros.FirstOrDefault(m => m.Name == macro) ?? new IMacro<TContext>.Unknown();
        }
    }
}