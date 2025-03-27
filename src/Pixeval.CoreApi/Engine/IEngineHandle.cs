// Copyright (c) Mako.
// Licensed under the GPL v3 License.

namespace Mako.Engine;

/// <summary>
/// Represents a class that is capable of tracking its own lifetime, any class that
/// implements <see cref="IEngineHandleSource" /> must exposes an <see cref="EngineHandle" />
/// that can be used to cancel itself or report the completion
/// </summary>
public interface IEngineHandleSource
{
    EngineHandle EngineHandle { get; }
}
