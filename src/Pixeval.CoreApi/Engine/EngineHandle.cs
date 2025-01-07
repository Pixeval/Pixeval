// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.CoreApi.Engine;

#pragma warning disable 660,661 // Object.Equals() and Object.GetHashCode() are not overwritten
public class EngineHandle : ICancellable, INotifyCompletion, ICompletionCallback<EngineHandle>
#pragma warning restore 660,661
{
    public static readonly EngineHandle Default = new(Guid.Empty);

    private readonly Action<EngineHandle>? _onCompletion;

    public bool Equals(EngineHandle other)
    {
        return Id == other.Id && IsCancelled == other.IsCancelled && IsCompleted == other.IsCompleted;
    }

    /// <summary>
    /// 搜索引擎的唯一ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 指示该句柄对应的搜索引擎是否已经被取消
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// 指示该句柄对应的搜索引擎是否已经结束运行
    /// </summary>
    public bool IsCompleted { get; set; }

    public EngineHandle(Guid id, Action<EngineHandle>? onCompletion = null)
    {
        _onCompletion = onCompletion;
        Id = id;
        IsCancelled = false;
        IsCompleted = false;
    }

    public EngineHandle(Action<EngineHandle> onCompletion)
    {
        _onCompletion = onCompletion;
        Id = Guid.NewGuid();
        IsCancelled = false;
        IsCompleted = false;
    }

    /// <summary>
    /// 取消该句柄对应的搜索引擎的运行
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }

    /// <summary>
    /// 设置该句柄对应的搜索引擎的状态为已完成，并执行注册的结束回调
    /// </summary>
    public void Complete()
    {
        IsCompleted = true;
        OnCompletion(this);
    }

    public static bool operator ==(EngineHandle lhs, EngineHandle rhs) => Equals(lhs, rhs);

    public static bool operator !=(EngineHandle lhs, EngineHandle rhs) => !Equals(lhs, rhs);

    public void OnCompletion(EngineHandle engineHandle)
    {
        _onCompletion?.Invoke(engineHandle);
    }
}
