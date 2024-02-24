using System;

// ReSharper disable once CheckNamespace
namespace Pixeval.Download.MacroParser;

public class MacroParseException(string? message) : Exception(message);
