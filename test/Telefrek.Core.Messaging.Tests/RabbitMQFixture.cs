using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Xunit;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Telefrek.Core.Messaging.Tests
{
    /// <summary>
    /// Provides the collection definition
    /// </summary>
    [CollectionDefinition("RabbitMQ")]
    public class RabbitMQFixtureCollection : ICollectionFixture<RabbitMQFixture> { }

    /// <summary>
    /// Fixture class for running integration tests with RabbitMQ/Docker
    /// </summary>
    /// <remarks>
    /// Inspired by https://www.meziantou.net/integration-testing-using-a-docker-container.htm
    /// </remarks>
    public class RabbitMQFixture : IDisposable
    {
        readonly DockerClientConfiguration _config;
        readonly DockerClient _client;

        const string ContainerName = "rmq-test";
        const string ImageName = "rabbitmq";
        const string ImageTag = "management";

        /// <summary>
        /// Constructor for setup
        /// </summary>
        public RabbitMQFixture()
        {
            Console.WriteLine("Creating fixture");

            // Setup the core components
            _config = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));
            _client = _config.CreateClient();

            // Cleanup any old instances of the RabbitMQ database
            CleanupQueues().Wait();

            // Start the new container
            StartDB().Wait();

            // Create test resources
        }

        /// <summary>
        /// Implements disposable, cleanup all database resources here
        /// </summary>
        public void Dispose()
        {
            // Cleanup any old instances of the RabbitMQ database
            using (_config)
            using (_client)
                CleanupQueues().Wait();
        }

        /// <summary>
        /// SHelper method to ensure the image is setup correctly
        /// </summary>
        /// <returns>A Task for tracking completion.</returns>
        async Task StartDB()
        {
            Console.WriteLine("Starting fixture");

            Console.WriteLine("Downloading image");
            // Download image
            await _client.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = ImageName, Tag = ImageTag }, new AuthConfig(), new Progress<JSONMessage>());

            // Create the container
            var config = new Config()
            {
                Hostname = "localhost"
            };

            // Configure the ports to expose
            var hostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "5672/tcp", new List<PortBinding> {
                                new PortBinding {
                                    HostIP = "127.0.0.1",
                                    HostPort = "5672"
                                }
                            }
                        },
                        {
                            "15672/tcp", new List<PortBinding> {
                                new PortBinding {
                                    HostIP = "127.0.0.1",
                                    HostPort = "15672"
                                }
                            }
                        }
                    },
                NetworkMode = "host"
            };

            Console.WriteLine("Creating container");
            // Create the container
            var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters(config)
            {
                Image = ImageName + ":" + ImageTag,
                Name = ContainerName,
                Tty = false,
                HostConfig = hostConfig,
            }).ConfigureAwait(false);

            // Get the container object
            Console.WriteLine("Searching for container");
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).ConfigureAwait(false);
            var container = containers.First(c => c.ID == response.ID);

            Console.WriteLine("Checking state {0}", container.State);
            // Start the container is needed
            if (container.State != "running")
            {
                Console.WriteLine("Starting container");
                var started = await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters()).ConfigureAwait(false);
                if (!started)
                    throw new InvalidOperationException("Failed to start the container");
            }

            // Wait for the RabbitMQ database to be available
            Console.Write("Waiting for RabbitMQ to startup");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("guest:guest")));

            // Loop until it works
            for (var i = 0; i < 25; ++i)
            {
                try
                {
                    var resp = await client.GetAsync("http://localhost:15672/api/overview").ConfigureAwait(false);
                    if (resp.StatusCode == HttpStatusCode.OK)
                        break;
                    else await Task.Delay(2000).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    Console.Write(".");
                    await Task.Delay(2000).ConfigureAwait(false);
                }

                if (i == 24) throw new InvalidOperationException("Failed to start RabbitMQ!");
            }


            Console.WriteLine("Ready");
        }

        /// <summary>
        /// Helper method for cleaning up database instances
        /// </summary>
        /// <returns>A Task for tracking completion.</returns>
        async Task CleanupQueues()
        {
            Console.WriteLine("Cleaning up fixture");

            Console.WriteLine("Searching for container");
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).ConfigureAwait(false);
            var container = containers.FirstOrDefault(c => c.Names.Contains("/" + ContainerName));

            if (container != null)
            {
                Console.WriteLine("Container found, stopping");
                await _client.Containers.StopContainerAsync(container.ID, new ContainerStopParameters()).ConfigureAwait(false);

                Console.WriteLine("Cleaning up container resources");
                await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true, RemoveVolumes = true }).ConfigureAwait(false);
            }
        }
    }
}
