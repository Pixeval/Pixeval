namespace Pixeval.Download.MacroParser.Ast
{
    public interface IMetaPathNode<TContext>
    {
        string Evaluate(IMetaPathMacroProvider<TContext> env, TContext context);
    }
}