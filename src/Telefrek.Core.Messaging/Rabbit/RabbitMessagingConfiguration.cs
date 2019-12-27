namespace Telefrek.Core.Messaging.Rabbit
{
    /// <summary>
    /// RabbitMQ messaging configuration information
    /// </summary>
    public class RabbitMessagingConfiguration
    {
        /// <summary>
        /// The username to use
        /// </summary>
        /// <value></value>
        public string Username { get; set; } = "guest";

        /// <summary>
        /// The password for the user
        /// </summary>
        /// <value></value>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// The host to connect to
        /// </summary>
        /// <value></value>
        public string Hostname { get; set; } = "localhost";

        /// <summary>
        /// The port to connect with
        /// </summary>
        /// <value></value>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// The virtual host to work in
        /// </summary>
        /// <value></value>
        public string VHost { get; set; } = "/";
    }
}