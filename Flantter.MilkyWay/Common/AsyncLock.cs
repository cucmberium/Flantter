using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// From WinRTXamlToolkit

namespace Flantter.MilkyWay.Common
{
    public class AsyncSemaphore
    {
        private readonly static Task CompletedTask = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private int _currentCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSemaphore" /> class, reserving some concurrent entries.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">initialCount</exception>
        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount");
            }

            _currentCount = initialCount;
        }

        /// <summary>
        /// Blocks the current awaiter until the semaphore receives a signal.
        /// </summary>
        /// <returns></returns>
        public Task WaitAsync()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;

                    return CompletedTask;
                }

                var waiter = new TaskCompletionSource<bool>();
                _waiters.Enqueue(waiter);

                return waiter.Task;
            }
        }

        /// <summary>
        /// Exits the semaphore and returns the previous count.
        /// </summary>
        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;

            lock (_waiters)
            {
                if (_waiters.Count > 0)
                {
                    toRelease = _waiters.Dequeue();
                }
                else
                {
                    ++_currentCount;
                }
            }

            if (toRelease != null)
            {
                toRelease.SetResult(true);
            }
        }
    }

    public class AsyncLock
    {
        private readonly AsyncSemaphore _semaphore;
        private readonly Task<Releaser> _releaser;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock" /> class.
        /// </summary>
        public AsyncLock()
        {
            _semaphore = new AsyncSemaphore(1);
            _releaser = Task.FromResult(new Releaser(this));
        }

        /// <summary>
        /// Waits for an open slot, then returns a disposable Releaser struct to be used in a using block marking the critical section.
        /// </summary>
        /// <returns></returns>
        public Task<Releaser> LockAsync()
        {
            var wait = _semaphore.WaitAsync();

            return wait.IsCompleted ?
                _releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Disposable Releaser to make it easy to use the AsyncLock in a scoped manner with a using block.
        /// </summary>
        public struct Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;

            internal Releaser(AsyncLock toRelease) { _toRelease = toRelease; }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (_toRelease != null)
                    _toRelease._semaphore.Release();
            }
        }
    }
}
