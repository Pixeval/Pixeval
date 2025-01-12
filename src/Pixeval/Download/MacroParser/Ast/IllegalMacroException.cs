// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Download.MacroParser.Ast;

public class IllegalMacroException(string? message) : Exception(message);