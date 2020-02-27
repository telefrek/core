using System.Threading.Tasks;
using Xunit;
using Cassandra;
using System;
using Telefrek.Core.Cassandra;

namespace Telefrek.Core.Cassandra.Tests
{
    [Collection("Cassandra")]
    public class CassandraSimpleTests
    {
        readonly CassandraFixture _fixture;
        readonly CassandraConfiguration _config;

        public CassandraSimpleTests(CassandraFixture fixture)
        {
            _fixture = fixture;
            _config = new CassandraConfiguration();
        }

        [Fact]
        public async Task CanConnect()
        {
            var provider = new CassandraSessionProvider(_config);

            Console.WriteLine("Connecting...");
            using (var session = await provider.GetAsync().ConfigureAwait(false))
            {
                Console.WriteLine("Getting keyspaces...");
                var res = await session.ExecuteAsync(new SimpleStatement("SELECT * FROM system_schema.keyspaces")).ConfigureAwait(false);
                Assert.NotNull(res);

                foreach (var r in res)
                {
                    Console.WriteLine("Found keyspace: {0}", r.GetValue<string>(0));
                }
            }
        }


        [Fact]
        public async Task CanManageNamespaces()
        {
            var provider = new CassandraSessionProvider(_config);

            Console.WriteLine("Connecting...");
            using (var session = await provider.GetAsync().ConfigureAwait(false))
            {
                Console.WriteLine("Getting keyspaces...");
                var res = await session.ExecuteAsync(new SimpleStatement("SELECT * FROM system_schema.keyspaces")).ConfigureAwait(false);
                Assert.NotNull(res);

                foreach (var r in res)
                {
                    Console.WriteLine("Found keyspace: {0}", r.GetValue<string>(0));
                }
            }
        }
    }
}