#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/EngineHandle.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

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
