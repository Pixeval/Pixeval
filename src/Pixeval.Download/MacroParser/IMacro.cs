// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;

namespace Pixeval.Download.MacroParser;

public interface IMacro
{
    string Name { get; }

    string Description { get; }
}

public sealed record Unknown : IMacro
{
    public string Name => "";

    public string Description => "";
}

public interface IPredicate : IMacro
{
    Type ContextType { get; }

    bool Match(object? context);
}

public interface IPredicate<in TContext> : IPredicate
{
    Type IPredicate.ContextType => typeof(TContext);

    bool IPredicate.Match(object? context) => Match((TContext) context!);

    bool Match(TContext context);
}

public interface ITransducer : IMacro
{
    Type ContextType { get; }

    bool IsFormatterValid(string? formatter);

    string Substitute(object? context, string? formatter, out bool includeToken);
}

public interface ITransducer<in TContext> : ITransducer
{
    Type ITransducer.ContextType => typeof(TContext);

    string ITransducer.Substitute(object? context, string? formatter, out bool includeToken) => Substitute((TContext) context!, formatter, out includeToken);

    string Substitute(TContext context, string? formatter, out bool includeToken);
}

public interface IContextRestrictedMacro : IMacro
{
    MacroContextPredicate ContextPredicate { get; }
}

public delegate bool MacroContextPredicate(IReadOnlyDictionary<string, bool> context);

public interface ILastSegment : ITransducer
{
    const string NameConst = "";

    const string NameConstToken = $"<{NameConst}>";
}
