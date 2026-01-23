// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Download.MacroParser;

public interface IMacro
{
    string Name { get; }
}

public sealed record Unknown : IMacro
{
    public string Name => "";
}

public interface IPredicate : IMacro
{
    bool IsNot { get; internal set; }
}

public interface IPredicate<in TContext> : IPredicate
{
    bool Match(TContext context);
}

public interface ITransducer : IMacro;

public interface ITransducer<in TContext> : ITransducer
{
    string Substitute(TContext context);
}

public interface ILastSegment : ITransducer
{
    const string NameConst = "";

    const string NameConstToken = $"<{NameConst}>";
}
