using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Telefrek.Core.Json;

namespace Telefrek.Core.Messaging.Rabbit
{
    /// <summary>
    /// RabbitMQ implementation for message subscriptions
    /// </summary>
    internal class RabbitSubscriber : IMessageSubscriber
    {

        IModel _model;
        ILogger _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="log">The log to write to</param>
        /// <param name="model">The model to operate on</param>
        /// <param name="binding">The queue binding information</param>
        public RabbitSubscriber(ILogger log, IModel model, MessageQueueBinding binding)
        {
            Binding = binding;
            _model = model;
            _log = log;
        }

        public MessageQueueBinding Binding { get; private set; }

        /// <inheritdoc/>
        public void AddListener(AsyncMessagingListener listener)
        {
            // Create a consumer and setup the processing logic
            var consumer = new AsyncEventingBasicConsumer(_model);
            consumer.Received += async (model, messageDetails) =>
            {
                try
                {
                    // Create a message
                    var msg = new RabbitMessage();
                    msg.MessageId = messageDetails.DeliveryTag;
                    msg.State = MessageState.NEW;
                    msg.Created = DateTime.FromFileTimeUtc(messageDetails.BasicProperties.Timestamp.UnixTime);
                    msg.CorrelationId = string.IsNullOrWhiteSpace(messageDetails.BasicProperties.CorrelationId) ? Guid.Empty :
                        Guid.Parse(messageDetails.BasicProperties.CorrelationId);

                    // Update headers
                    msg.Headers = new Dictionary<string, string>();
                    foreach (var header in messageDetails.BasicProperties.Headers ?? new Dictionary<string, object>())
                        msg.Headers.Add(header.Key, header.Value.ToString());
                    msg.DeliveryCount = messageDetails.Redelivered ? 1 : 0;

                    // Check the content type
                    switch (messageDetails.BasicProperties.ContentType)
                    {
                        case "application/text":
                            msg.StringValue = Encoding.UTF8.GetString(messageDetails.Body);
                            break;
                        case "application/json":
                            msg.JsonValue = messageDetails.Body.AsJson();
                            break;
                        default:
                            _log.LogError("Unknown Content-Type: [{0}], downgrading to string", messageDetails.BasicProperties.ContentType);
                            msg.StringValue = Encoding.UTF8.GetString(messageDetails.Body);
                            break;
                    }

                    // Invoke the handler and then update the message state
                    try
                    {
                        await listener.HandleMessageAsync(msg).ConfigureAwait(false);

                        // Check for unhandled state
                        if (msg.State == MessageState.NEW)
                            msg.State = MessageState.SUCCESS;
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Failed to handle exception during message processing [{0}]", messageDetails.DeliveryTag);
                        msg.State = MessageState.ABORTED;
                    }
                    await TryUpdateStateAsync(msg);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to handle exception during message building [{0}]", messageDetails.DeliveryTag);
                }
            };

            _model.BasicConsume(queue: Binding.BindingId,
                                 autoAck: false,
                                 consumer: consumer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _model.Close();
            _model.Dispose();
        }

        /// <summary>
        /// Attempts to update the message state appropriately
        /// </summary>
        /// <param name="message">The message to acknowledge</param>
        /// <typeparam name="T">The type of message</typeparam>
        /// <returns>True if the update was successful</returns>
        Task<bool> TryUpdateStateAsync<T>(T message) where T : IMessage
        {
            try
            {
                switch (message.State)
                {
                    case MessageState.ABORTED:
                        _model.BasicNack((ulong)message.MessageId, false, true);
                        break;
                    case MessageState.ABANDONED:
                        _model.BasicNack((ulong)message.MessageId, false, true);
                        break;
                    case MessageState.SUCCESS:
                        _model.BasicAck((ulong)message.MessageId, false);
                        break;
                    default:
                        _log.LogWarning("Invalid update state [{0}] on message [{1}]", message.State, message.MessageId);
                        break;
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to update state [{0}] on message [{1}]", message.State, message.MessageId);
            }

            return Task.FromResult(false);
        }
    }
}