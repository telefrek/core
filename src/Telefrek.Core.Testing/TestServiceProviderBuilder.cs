using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Telefrek.Core.Testing
{
    public class TestServiceProviderBuilder
    {
        ServiceCollection _collection = new ServiceCollection();
        ITestOutputHelper _outputHelper;

        public TestServiceProviderBuilder WithLogging(ITestOutputHelper helper)
        {
            _collection.AddLogging();
            _outputHelper = helper;
            return this;
        }

        public ServiceProvider Build()
        {
            var provider = _collection.BuildServiceProvider();

            if (_outputHelper != null)
            {
                var factory = provider.GetRequiredService<ILoggerFactory>();
                factory.AddProvider(new XunitLoggerProvider(_outputHelper));
            }

            return provider;
        }
    }
}