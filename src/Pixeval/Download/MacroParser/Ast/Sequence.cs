namespace Pixeval.Download.MacroParser.Ast
{
    public record Sequence<TContext>(SingleNode<TContext> First, Sequence<TContext>? Remains) : IMetaPathNode<TContext>
    {
        public string Evaluate(IMetaPathMacroProvider<TContext> env, TContext context)
        {
            return First.Evaluate(env, context) + (Remains?.Evaluate(env, context) ?? string.Empty);
        }
    }
}