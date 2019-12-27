using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Telefrek.Core.Patterns;

namespace Telefrek.Core.Messaging.Rabbit
{
    public class RabbitMessagingFactory : IMessagingFactory
    {
        // Static members
        private const string TOPIC_EXCHANGE_TYPE = "fanout";

        /// <summary>
        /// Private connection manager
        /// </summary>
        private class ConnectionProvider : ProviderBase<IConnection>
        {
            /// <summary>
            /// Factory for creating connections
            /// </summary>
            readonly ConnectionFactory _factory;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="factory">The factory to use</param>
            public ConnectionProvider(ConnectionFactory factory) => _factory = factory;

            /// <inheritdoc/>
            protected override Task<IConnection> Create() => Task.FromResult(_factory.CreateConnection(Environment.MachineName));
        }

        // Member variables
        readonly ConnectionProvider _provider;
        readonly ILogger _log;

        /// <summary>
        /// Dependency injection constructor
        /// </summary>
        /// <param name="log">The log to use</param>
        /// <param name="rabbitMessagingConfiguration">The configuration to use</param>
        public RabbitMessagingFactory(ILogger log, RabbitMessagingConfiguration rabbitMessagingConfiguration)
        {
            log.LogInformation("Creating factory");

            _provider = new ConnectionProvider(new ConnectionFactory
            {
                DispatchConsumersAsync = true,
                HostName = rabbitMessagingConfiguration.Hostname,
                UserName = rabbitMessagingConfiguration.Username,
                Password = rabbitMessagingConfiguration.Password,
                VirtualHost = rabbitMessagingConfiguration.VHost,
                AutomaticRecoveryEnabled = true,
            });

            _log = log;
        }

        /// <inheritdoc/>
        public T CreateMessage<T>() where T : class, IMessage
        {
            if (typeof(ITextMessage).IsAssignableFrom(typeof(T)) || typeof(IJsonMessage).IsAssignableFrom(typeof(T)))
                return new RabbitMessage() as T;

            return default(T);
        }

        /// <inheritdoc/>
        public async Task<IMessagePublisher> CreatePublisherAsync(MessageQueue queue)
        {
            _log.LogInformation("Creating publisher for [{0}]", queue.Name);
            var conn = await _provider.GetAsync().ConfigureAwait(false);
            var model = conn.CreateModel();

            // Ensure the exchange exists
            model.ExchangeDeclare(queue.Name, TOPIC_EXCHANGE_TYPE, queue.Durable, queue.Transient);

            return new RabbitPublisher(_log, model, queue);
        }

        /// <inheritdoc/>
        public async Task<IMessageSubscriber> CreateSubscriberAsync(MessageQueueBinding binding)
        {
            _log.LogInformation("Creating subscriber for [{0}] using filter [{1}]", binding.Queue.Name, binding.MessageFilter);

            var conn = await _provider.GetAsync().ConfigureAwait(false);
            var model = conn.CreateModel();

            // Ensure the exchange exists
            model.ExchangeDeclare(binding.Queue.Name, TOPIC_EXCHANGE_TYPE, binding.Queue.Durable, false);

            // Ensure the queue exists
            var queue = model.QueueDeclare(binding.BindingId, binding.Queue.Durable, binding.Exclusive, binding.Queue.Transient);

            // Ensure the binding is setup
            model.QueueBind(queue.QueueName, binding.Queue.Name, binding.MessageFilter);

            return new RabbitSubscriber(_log, model, binding);
        }

        /// <inheritdoc/>
        public void Dispose() => _provider.GetAsync().Result.Dispose();
    }
}