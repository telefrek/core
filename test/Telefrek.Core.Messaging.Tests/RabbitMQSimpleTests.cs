using System;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.Core.Messaging.Rabbit;
using Telefrek.Core.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Telefrek.Core.Messaging.Tests
{
    [Collection("RabbitMQ")]
    public class RabbitMQSimpleTests
    {
        readonly RabbitMQFixture _fixture;
        readonly ITestOutputHelper _output;

        public RabbitMQSimpleTests(RabbitMQFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task VerifyMessageCreation()
        {
            var provider = new TestServiceProviderBuilder()
                .WithLogging(_output)
                .Build();
            var factory = provider.CreateInstance<RabbitMessagingFactory>(new RabbitMessagingConfiguration());

            var msg = factory.CreateMessage<ITextMessage>();
            Assert.NotNull(msg);
            msg.StringValue = "Hello World";

            var publisher = await factory.CreatePublisherAsync(new MessageQueue { Name = "foo" }).ConfigureAwait(false);
            Assert.NotNull(publisher);

            var subscriber = await factory.CreateSubscriberAsync(new MessageQueueBinding { Queue = new MessageQueue { Name = "foo" }, BindingId = "test", MessageFilter = "#" }).ConfigureAwait(false);
            Assert.NotNull(subscriber);

            var listener = new TestListener();
            var invoked = 0L;
            listener.OnMessageReceived = (tMessage) =>
            {
                Interlocked.Increment(ref invoked);
                tMessage.State = MessageState.SUCCESS;
                _output.WriteLine("Recceived message: [{0}]", (tMessage as ITextMessage).StringValue);
            };

            subscriber.AddListener(listener);

            var result = await publisher.TryPublishAsync(msg).ConfigureAwait(false);
            Assert.True(result, "Failed to send message");

            await Task.Delay(100).ConfigureAwait(false);
            Assert.Equal(1L, Interlocked.Read(ref invoked));
        }


        [Fact]
        public async Task VerifyReDelivery()
        {
            var provider = new TestServiceProviderBuilder()
                .WithLogging(_output)
                .Build();
            var factory = provider.CreateInstance<RabbitMessagingFactory>(new RabbitMessagingConfiguration());

            var msg = factory.CreateMessage<ITextMessage>();
            Assert.NotNull(msg);
            msg.StringValue = "ReDeliver Me";

            var publisher = await factory.CreatePublisherAsync(new MessageQueue { Name = "bar" });
            Assert.NotNull(publisher);

            var subscriber = await factory.CreateSubscriberAsync(new MessageQueueBinding { Queue = new MessageQueue { Name = "bar" }, BindingId = "redeliver", MessageFilter = "#" });
            Assert.NotNull(subscriber);

            var listener = new TestListener();
            var invoked = 0L;
            listener.OnMessageReceived = (tMessage) =>
            {
                tMessage.State = Interlocked.Increment(ref invoked) == 1L ? MessageState.ABORTED : MessageState.SUCCESS;
                _output.WriteLine("Recceived message: [{0}]", (tMessage as ITextMessage).StringValue);
            };

            subscriber.AddListener(listener);

            var result = await publisher.TryPublishAsync(msg);
            Assert.True(result, "Failed to send message");

            await Task.Delay(100);
            Assert.Equal(2L, Interlocked.Read(ref invoked));
        }

        class TestListener : AsyncMessagingListener
        {
            public override Task HandleMessageAsync<T>(T message)
            {
                if (OnMessageReceived != null)
                    OnMessageReceived(message);

                return Task.CompletedTask;
            }

            public Action<IMessage> OnMessageReceived { get; set; }
        }
    }
}
