using System;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.Core.Patterns;
using Cassandra;

namespace Telefrek.Core.Cassandra
{
    /// <summary>
    /// Class for managing session lifecycle
    /// </summary>
    public class CassandraSessionProvider : ProviderBase<ISession>
    {
        Cluster _cluster;

        public CassandraSessionProvider(CassandraConfiguration configuration)
        {
            _cluster = new CassandraConnectionStringBuilder
            {
                Port = configuration.Port,
                ContactPoints = configuration.Seeds,
                Username = configuration.Username,
                Password = configuration.Password,
                DefaultKeyspace = configuration.Keyspace,
                ClusterName = configuration.ClusterName
            }.MakeClusterBuilder().Build();
        }

        protected override async Task<ISession> Create() => await _cluster.ConnectAsync().ConfigureAwait(false);
    }
}
