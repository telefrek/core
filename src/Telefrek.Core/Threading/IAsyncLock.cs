using System;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Threading
{
    /// <summary>
    /// Performs asynchronous locking
    /// </summary>
    public interface IAsyncLock : IDisposable
    {
        /// <summary>
        /// Acquires the lock asynchronously
        /// </summary>
        /// <returns>True if the lock is acquired</returns>
        Task<bool> AcquireAsync();

        /// <summary>
        /// Acquires the lock asynchronously, waiting the maximum for the timeout
        /// </summary>
        /// <param name="timeout"></param>
        /// /// <returns>True if the lock is acquired</returns>
        Task<bool> AcquireAsync(TimeSpan timeout);

        /// <summary>
        /// Acquires the lock asynchronously
        /// </summary>
        /// <param name="token">A cancellation token for aborting</param>
        /// /// <returns>True if the lock is acquired</returns>
        Task<bool> AcquireAsync(CancellationToken token);

        /// <summary>
        /// Releases the lock
        /// </summary>
        void Release();
    }
}