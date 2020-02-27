namespace Telefrek.Core.Cassandra
{
    /// <summary>
    /// POCO class for defining Cassandra location and behavior
    /// </summary>
    public class CassandraConfiguration
    {
        /// <summary>
        /// Gets/Sets the port (Default 9042)
        /// </summary>
        /// <value></value>
        public int Port { get; set; } = 9042;

        /// <summary>
        /// Gets/Sets the seed endpoints (Default localhost)
        /// </summary>
        /// <value></value>
        public string[] Seeds { get; set; } = new[] { "localhost" };

        /// <summary>
        /// Gets/Sets the username
        /// </summary>
        /// <value></value>
        public string Username { get; set; }

        /// <summary>
        /// Gets/Sets the password
        /// </summary>
        /// <value></value>
        public string Password { get; set; }

        /// <summary>
        /// Gets/Sets the default keyspace
        /// </summary>
        /// <value></value>
        public string Keyspace { get; set; }

        /// <summary>
        /// Gets/Sets the cluster name
        /// </summary>
        /// <value></value>
        public string ClusterName { get; set; }
    }
}