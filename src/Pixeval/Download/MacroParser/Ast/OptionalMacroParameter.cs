namespace Pixeval.Download.MacroParser.Ast
{
    public record OptionalMacroParameter<TContext>(Sequence<TContext>? Content) : IMetaPathNode<TContext>
    {
        public string Evaluate(IMetaPathMacroProvider<TContext> env, TContext context)
        {
            return Content?.Evaluate(env, context) ?? string.Empty;
        }
    }
}