namespace Pixeval.Download.MacroParser
{
    public interface IMetaPathParser<TContext>
    {
        IMetaPathMacroProvider<TContext> MacroProvider { get; }

        string Reduce(string raw, TContext context);
    }
}