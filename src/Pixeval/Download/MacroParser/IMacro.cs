namespace Pixeval.Download.MacroParser
{
    public interface IMacro<TContext>
    {
        public string Name { get; }

        public sealed record Unknown : IMacro<TContext>
        {
            public string Name => string.Empty;
        }

        public interface IPredicate : IMacro<TContext>
        {
            bool Match(TContext context);
        }

        public interface ITransducer : IMacro<TContext>
        {
            string Substitute(TContext context);
        }
    }
}