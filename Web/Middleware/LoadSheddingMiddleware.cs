using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Telefrek.Web.Middleware
{
    /// <summary>
    /// Middleware for implementing load shedding
    /// </summary>
    public sealed class LoadSheddingMiddleware
    {
        readonly RequestDelegate _next;
        readonly ILogger<LoadSheddingMiddleware> _log;
        readonly LoadSheddingConfiguration _config;
        readonly SemaphoreSlim _concurrencyLimit;
        readonly Queue<TaskCompletionSource<bool>> _backlog;
        readonly Task _threadScheduler;
        volatile int _maxQueueDepth;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next">The next delegate in the chain</param>
        /// <param name="log">The log to use for messaging</param>
        /// <param name="options">The available configuration options</param>
        public LoadSheddingMiddleware(RequestDelegate next, ILogger<LoadSheddingMiddleware> log, IOptions<LoadSheddingConfiguration> options)
        {
            _next = next;
            _log = log;
            _config = options == null ? new LoadSheddingConfiguration() : options.Value ?? new LoadSheddingConfiguration();

            _concurrencyLimit = new SemaphoreSlim(_config.MaxConcurrentRequests > 0 ? _config.MaxConcurrentRequests : 1);
            _backlog = new Queue<TaskCompletionSource<bool>>();

            _maxQueueDepth = _config.MaxQueueDepth > 0 ? _config.MaxQueueDepth : 1;
            _threadScheduler = Task.Factory.StartNew(async () =>
            {
                await ConcurrencyLimit();
            }, TaskCreationOptions.LongRunning).Unwrap();
        }

        /// <summary>
        /// Thread
        /// </summary>
        /// <returns></returns>
        async Task ConcurrencyLimit()
        {
            TaskCompletionSource<bool> src;

            while (true)
            {
                // Wait for a thread
                await _concurrencyLimit.WaitAsync();

                // Lock to sync threads and start next request
                lock (_backlog)
                {
                    // If we can dequeue a message, process else release the semaphore
                    if (_backlog.TryDequeue(out src))
                    {
                        src.SetResult(true);
                        continue;
                    }
                }

                await Task.Delay(100);
                _concurrencyLimit.Release();
            }
        }

        /// <summary>
        /// Internal method for controlling queues/concurrency
        /// </summary>
        /// <returns>A task that has delayed execution</returns>
        Task<bool> QueueRequest()
        {
            var source = new TaskCompletionSource<bool>();

            if (_config.IsAdaptive || _config.MaxQueueDepth > 0)
            {
                lock (_backlog)
                {
                    if (_backlog.Count >= _config.MaxQueueDepth)
                    {
                        if (_config.Strategy == LoadSheddingStrategy.Head)
                        {
                            var head = _backlog.Dequeue();
                            head.SetResult(false);
                            _backlog.Enqueue(source);

                        }
                        else source.SetResult(false);
                    }
                    else _backlog.Enqueue(source);
                }
            }
            else if (_config.MaxConcurrentRequests > 0)
            {
                // Try to just get the concurrency semaphore
                if (_concurrencyLimit.Wait(_config.MaxLatencyMS))
                    source.SetResult(true);
                else
                    source.SetResult(false);
            }
            else source.SetResult(true);

            return source.Task;
        }

        /// <summary>
        /// Invoke the middleware
        /// </summary>
        /// <param name="context">The current HttpContext for the call</param>
        /// <returns>A Task</returns>
        public async Task Invoke(HttpContext context)
        {
            // Start timing for tracking latency, etc.
            var sw = Stopwatch.StartNew();

            // Start trying to queue the task
            var queueTask = QueueRequest();

            // Try to queue the request
            if (await queueTask)
            {
                try
                {
                    // Invoke the next task
                    await _next.Invoke(context);
                }
                finally
                {
                    // Ensure we release, otherwise it's gonna go BOOM!
                    _concurrencyLimit.Release();
                }
            }
            else
                context.Response.StatusCode = _config.StatusCode;

            // Done processing, track stats
            var elapsed = sw.ElapsedMilliseconds;
        }
    }

    /// <summary>
    /// Identifies how the load shedding middleware should handle incoming requests
    /// </summary>
    public enum LoadSheddingStrategy
    {
        /// <summary>
        /// Don't shed anything
        /// </summary>
        None,
        /// <summary>
        /// Tail shedding (queued requests first)
        /// </summary>
        Tail,
        /// <summary>
        /// Head shedding (drop before queue)
        /// </summary>
        Head,
    }

    /// <summary>
    /// Load shedding configuration block
    /// </summary>
    public class LoadSheddingConfiguration
    {
        /// <summary>
        /// Choose the strategy
        /// </summary>
        public LoadSheddingStrategy Strategy { get; set; }

        /// <summary>
        /// Sets the target latency maximum
        /// </summary>
        public int MaxLatencyMS { get; set; } = 1000;

        /// <summary>
        /// Sets the maximum number of concurrent requests
        /// </summary>
        public int MaxConcurrentRequests { get; set; }

        /// <summary>
        /// Sets the maximum number of requests to queue at any time
        /// </summary>
        public int MaxQueueDepth { get; set; }

        /// <summary>
        /// Sets the default status code for requests that are shed
        /// </summary>
        public int StatusCode { get; set; } = 429;

        /// <summary>
        /// Indicates if the load shedding can manipulate values to maintain latency targets
        /// </summary>
        public bool IsAdaptive { get; set; }
    }
}