#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/EngineHandle.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Threading;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Engine
{
    [PublicAPI]
#pragma warning disable 660,661 // Object.Equals() and Object.GetHashCode() are not overwritten
    public struct EngineHandle : ICancellable, INotifyCompletion, ICompletionCallback<EngineHandle>
#pragma warning restore 660,661
    {
        private readonly Action<EngineHandle>? _onCompletion;

        public bool Equals(EngineHandle other)
        {
            return Id == other.Id && CancellationTokenSource.IsCancellationRequested == other.CancellationTokenSource.IsCancellationRequested && IsCompleted == other.IsCompleted;
        }

        /// <summary>
        ///     搜索引擎的唯一ID
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     指示该句柄对应的搜索引擎是否已经被取消
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        ///     指示该句柄对应的搜索引擎是否已经结束运行
        /// </summary>
        public bool IsCompleted { get; set; }

        public EngineHandle(Guid id, Action<EngineHandle>? onCompletion = null)
        {
            _onCompletion = onCompletion;
            Id = id;
            CancellationTokenSource = new CancellationTokenSource();
            IsCompleted = false;
        }

        public EngineHandle(Action<EngineHandle> onCompletion)
        {
            _onCompletion = onCompletion;
            Id = Guid.NewGuid();
            CancellationTokenSource = new CancellationTokenSource();
            IsCompleted = false;
        }

        /// <summary>
        ///     取消该句柄对应的搜索引擎的运行
        /// </summary>
        public void Cancel()
        {
            CancellationTokenSource.Cancel(true);
        }

        /// <summary>
        ///     设置该句柄对应的搜索引擎的状态为已完成，并执行注册的结束回调
        /// </summary>
        public void Complete()
        {
            IsCompleted = true;
            OnCompletion(this);
        }

        public static bool operator ==(EngineHandle lhs, EngineHandle rhs)
        {
            return lhs.Id == rhs.Id && lhs.CancellationTokenSource.IsCancellationRequested == rhs.CancellationTokenSource.IsCancellationRequested && lhs.IsCompleted == rhs.IsCompleted;
        }

        public static bool operator !=(EngineHandle lhs, EngineHandle rhs)
        {
            return !(lhs == rhs);
        }

        public void OnCompletion(EngineHandle engineHandle)
        {
            _onCompletion?.Invoke(engineHandle);
        }
    }
}