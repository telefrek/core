using System;
using System.Threading.Tasks;
using Telefrek.Core.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Telefrek.Core.Tests.Threading
{
    public class AsyncLockTests
    {
        private readonly ITestOutputHelper output;
        public AsyncLockTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        [Fact(Timeout = 2000)]
        public async Task AsyncRecursionSucceeds()
        {
            var rlock = new ReEntrantAsyncLock();
            await TryAcquire(rlock);
            Assert.True(await rlock.AcquireAsync());
            rlock.Release();

            Task.Factory.StartNew(async () =>
            {
                await TryAcquire(rlock);
            }).Unwrap().Wait();
        }

        [Fact(Timeout = 2000)]
        public async Task AsyncLockBlocksOtherTasks()
        {
            var rlock = new ReEntrantAsyncLock();
            var second = Task.Run(async () =>
            {
                Assert.True(await rlock.AcquireAsync(TimeSpan.FromMilliseconds(500)));
                await Task.Delay(1000);
                rlock.Release();
            });

            var first = Task.Run(async () =>
            {
                await Task.Delay(100);

                Assert.False(await rlock.AcquireAsync(TimeSpan.FromMilliseconds(100)));
                await second;

                await TryAcquire(rlock);

            });


            await first;
            await second;
        }

        public async Task<bool> TryAcquire(IAsyncLock asyncLock)
        {
            Assert.True(await asyncLock.AcquireAsync(TimeSpan.FromMilliseconds(500)));
            try
            {
                return true;
            }
            finally
            {
                asyncLock.Release();
            }
        }
    }
}