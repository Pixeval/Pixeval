namespace Pixeval.Download.MacroParser.Ast
{
    public abstract record SingleNode<TContext> : IMetaPathNode<TContext>
    {
        public abstract string Evaluate(IMetaPathMacroProvider<TContext> env, TContext context);
    }
}