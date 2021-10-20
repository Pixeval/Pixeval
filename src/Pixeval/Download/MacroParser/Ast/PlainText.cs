namespace Pixeval.Download.MacroParser.Ast
{
    public record PlainText<TContext>(string Text) : SingleNode<TContext>
    {
        public override string Evaluate(IMetaPathMacroProvider<TContext> env, TContext context)
        {
            return Text;
        }
    }
}