// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixeval.Objects
{
    public class AsyncSemaphore
    {
        private static readonly Task SCompleted = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> mWaiters = new Queue<TaskCompletionSource<bool>>();
        private int mCurrentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
            mCurrentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (mWaiters)
            {
                if (mCurrentCount > 0)
                {
                    --mCurrentCount;
                    return SCompleted;
                }

                var waiter = new TaskCompletionSource<bool>();
                mWaiters.Enqueue(waiter);
                return waiter.Task;
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (mWaiters)
            {
                if (mWaiters.Count > 0)
                    toRelease = mWaiters.Dequeue();
                else
                    ++mCurrentCount;
            }

            toRelease?.SetResult(true);
        }
    }

    public class AsyncLock
    {
        private readonly Task<Release> mRelease;
        private readonly AsyncSemaphore mSemaphore;

        public AsyncLock()
        {
            mSemaphore = new AsyncSemaphore(1);
            mRelease = Task.FromResult(new Release(this));
        }

        public Task<Release> LockAsync()
        {
            var wait = mSemaphore.WaitAsync();
            return wait.IsCompleted
                ? mRelease
                : wait.ContinueWith((_, state) => new Release((AsyncLock) state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public struct Release : IDisposable
        {
            private readonly AsyncLock mToRelease;

            public Release(AsyncLock toRelease)
            {
                mToRelease = toRelease;
            }

            public void Dispose()
            {
                mToRelease?.mSemaphore.Release();
            }
        }
    }
}