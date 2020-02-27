using System.Threading;
using System.Threading.Tasks;
using Telefrek.Core.Threading;

namespace Telefrek.Core.Patterns
{

    /// <summary>
    /// Base implementation of the provider logic
    /// </summary>
    /// <typeparam name="T">The type of object managed by the provider</typeparam>
    public abstract class ProviderBase<T> : IProvider<T>
    {
        private T _instance = default(T);
        private IAsyncLock _syncLock = new ReEntrantAsyncLock();
        private bool _refreshing = false;

        /// <inheritdoc/>
        public async Task<T> GetAsync()
        {
            await _syncLock.AcquireAsync();
            try
            {
                if(_instance.IsNullOrDefault())
                    _instance = await Create();
                    
                return _instance;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task ResetAsync()
        {
            await _syncLock.AcquireAsync();
            try
            {
                _instance = await Create();
            }
            finally
            {
                _syncLock.Release();
            }
        }

        protected abstract Task<T> Create();
    }
}