using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Threading
{

    /// <summary>
    /// An <see cref="Telefrek.Core.Threading.IAsyncLock" /> implementation that supports re-entrant behaviors within an async chain
    /// </summary>
    public sealed class ReEntrantAsyncLock : IAsyncLock
    {
        private static long RE_ENTRANT_ID = 0L;

        private volatile int _currentLockId;
        private static AsyncLocal<int> _taskLockId = new AsyncLocal<int>();
        private volatile int _depth;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Asynchronous lock that supports re-entrancy
        /// </summary>
        public ReEntrantAsyncLock()
        {
            _currentLockId = 0;
            _depth = 0;
            _taskLockId.Value = -1;
        }

        /// <inheritdoc/>
        public async Task<bool> AcquireAsync()
        {
            if (_taskLockId.Value == _currentLockId)
            {
                Trace.WriteLine(string.Format("valid => {0}", ToString()));
                _depth++;
                return true;
            }
            Trace.WriteLine(string.Format("new => {0}", ToString()));
            _taskLockId.Value = (int)(Interlocked.Increment(ref RE_ENTRANT_ID) & 0x7FFFFFFF);
            await _semaphore.WaitAsync();
            _currentLockId = _taskLockId.Value;
            _depth++;
            Trace.WriteLine(string.Format("after => {0}", ToString()));

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> AcquireAsync(TimeSpan timeout)
        {
            if (_taskLockId.Value == _currentLockId)
            {
                Trace.WriteLine(string.Format("valid => {0}", ToString()));
                _depth++;
                return true;
            }
            else
            {
                _taskLockId.Value = (int)(Interlocked.Increment(ref RE_ENTRANT_ID) & 0x7FFFFFFF);
                Trace.WriteLine(string.Format("new => {0}", ToString()));
                ;
                if (await _semaphore.WaitAsync((int)timeout.TotalMilliseconds))
                {
                    _currentLockId = _taskLockId.Value;
                    _depth++;
                    Trace.WriteLine(string.Format("after => {0}", ToString()));
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> AcquireAsync(CancellationToken token)
        {

            if (_taskLockId.Value == _currentLockId)
            {
                Trace.WriteLine(string.Format("valid => {0}", ToString()));
                _depth++;
                return true;
            }
            
            try
            {
                Trace.WriteLine(string.Format("new => {0}", ToString()));
                _taskLockId.Value = (int)(Interlocked.Increment(ref RE_ENTRANT_ID) & 0x7FFFFFFF);
                await _semaphore.WaitAsync(token);
                _currentLockId = _taskLockId.Value;
                _depth++;
                Trace.WriteLine(string.Format("after => {0}", ToString()));
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }

        public override string ToString() => string.Format("({0}, {4}) -> {1} {2} {3} {5}", Task.CurrentId ?? -1, _depth, _currentLockId, _taskLockId.Value, Thread.CurrentThread.ManagedThreadId, _semaphore.CurrentCount);

        /// <inheritdoc/>
        public void Release()
        {
            if (--_depth == 0)
                _semaphore.Release();

        }

        /// <inheritdoc/>
        public void Dispose() => _semaphore.Dispose();
    }
}