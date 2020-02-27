using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Telefrek.Core.Json;

namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// RabbitMQ implementation for message publishing
    /// </summary>
    internal class RabbitPublisher : IMessagePublisher
    {
        readonly IModel _model;
        readonly ILogger _log;

        public MessageQueue Queue { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="log">The log to use</param>
        /// <param name="model">The model to handle messaging operations</param>
        /// <param name="queue">The queue to publish on</param>
        public RabbitPublisher(ILogger log, IModel model, MessageQueue queue)
        {
            Queue = queue;
            _model = model;
            _log = log;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _model.Close();
            _model.Dispose();
        }

        /// <inheritdoc/>
        public Task<bool> TryPublishAsync<T>(T message) where T : IMessage
        {
            // Create the basic properties
            var props = _model.CreateBasicProperties();
            props.DeliveryMode = 2;
            props.Headers = props.Headers ?? new Dictionary<string, object>();

            // Setup the headers
            foreach (var key in (message.Headers ?? new Dictionary<string, string>()).Keys)
                props.Headers.Add(key, message.Headers[key]);

            // Publish the message
            try
            {
                if (message is IJsonMessage && (message as IJsonMessage).JsonValue != null)
                {
                    props.ContentType = "application/json";
                    _model.BasicPublish(exchange: Queue.Name,
                        routingKey: message.RoutingKey ?? "",
                        basicProperties: props,
                        body: Encoding.UTF8.GetBytes((message as IJsonMessage).JsonValue.ToJson()));
                }
                else if (message is ITextMessage)
                {
                    props.ContentType = "application/text";
                    _model.BasicPublish(exchange: Queue.Name,
                        routingKey: message.RoutingKey ?? "",
                        basicProperties: props,
                        body: Encoding.UTF8.GetBytes((message as ITextMessage).StringValue ?? ""));

                }
                else
                    return Task.FromResult(false);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to publish message correlationId [{0}]", message.CorrelationId);
                return Task.FromResult(false);
            }
        }
    }
}