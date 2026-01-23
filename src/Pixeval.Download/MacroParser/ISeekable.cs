// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Download.MacroParser;

public interface ISeekable<T>
{
    void Seek(int pos);

    T Peek();

    void Advance();

    void Advance(int n);

    ReadOnlySpan<T> GetWindow();

    void AdvanceMarker();

    void ResetForward();

    void Return();
}
